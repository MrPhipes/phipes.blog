using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Seeding;

/// <summary>
/// Importa contenido desde una carpeta de archivos Markdown con front-matter
/// (<c>posts/*.md</c>, <c>projects/*.md</c>, <c>bio.md</c> + <c>bio-entries.json</c>) hacia la
/// base del blog. Idempotente: no duplica si el slug ya existe. Reutilizable por cualquier host
/// (phipes.web, pacificdev.cl) para precargar contenido.
/// </summary>
public sealed class MarkdownContentImporter(IPhipesBlogDbContext db, ILogger<MarkdownContentImporter>? logger = null)
{
    public async Task<int> ImportAsync(string contentRoot, CancellationToken ct = default)
    {
        if (!Directory.Exists(contentRoot))
        {
            logger?.LogWarning("Carpeta de contenido no encontrada: {Root}", contentRoot);
            return 0;
        }

        var imported = 0;
        imported += await ImportPostsAsync(Path.Combine(contentRoot, "posts"), ct);
        imported += await ImportProjectsAsync(Path.Combine(contentRoot, "projects"), ct);
        imported += await ImportBioAsync(contentRoot, ct);
        imported += await ImportResumeAsync(contentRoot, ct);
        return imported;
    }

    private async Task<int> ImportPostsAsync(string dir, CancellationToken ct)
    {
        if (!Directory.Exists(dir)) return 0;
        var n = 0;
        foreach (var file in Directory.EnumerateFiles(dir, "*.md"))
        {
            var (fm, body) = FrontMatter.Parse(await File.ReadAllTextAsync(file, ct));
            var slug = fm.Str("slug") ?? Path.GetFileNameWithoutExtension(file);
            if (await db.Posts.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug, ct)) continue;

            var published = fm.Date("publishedAt");
            db.Posts.Add(new BlogPost
            {
                Slug = slug,
                Title = fm.Str("title") ?? slug,
                Summary = fm.Str("summary") ?? string.Empty,
                BodyMarkdown = body,
                CoverImageUrl = fm.Str("coverImageUrl"),
                AuthorDisplayName = fm.Str("authorDisplayName") ?? string.Empty,
                IsFeatured = fm.Bool("isFeatured"),
                Status = ContentStatus.Published,
                PublishedAt = published,
                CreatedAt = published ?? DateTimeOffset.UtcNow,
                UpdatedAt = published ?? DateTimeOffset.UtcNow,
                Categories = await ResolveCategoriesAsync(fm.List("categories"), ct),
                Tags = await ResolveTagsAsync(fm.List("tags"), ct),
            });
            n++;
        }
        if (n > 0) await db.SaveChangesAsync(ct);
        return n;
    }

    private async Task<int> ImportProjectsAsync(string dir, CancellationToken ct)
    {
        if (!Directory.Exists(dir)) return 0;
        var n = 0;
        foreach (var file in Directory.EnumerateFiles(dir, "*.md"))
        {
            var (fm, body) = FrontMatter.Parse(await File.ReadAllTextAsync(file, ct));
            var slug = fm.Str("slug") ?? Path.GetFileNameWithoutExtension(file);
            if (await db.Projects.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug, ct)) continue;

            db.Projects.Add(new Project
            {
                Slug = slug,
                Title = fm.Str("title") ?? slug,
                Summary = fm.Str("summary") ?? string.Empty,
                BodyMarkdown = body,
                Outcome = fm.Str("outcome"),
                ClientName = fm.Str("clientName"),
                Role = fm.Str("role"),
                ProjectUrl = fm.Str("projectUrl"),
                CoverImageUrl = fm.Str("coverImageUrl"),
                IsFeatured = fm.Bool("isFeatured"),
                Status = ContentStatus.Published,
                PublishedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                TechStack = fm.List("techStack"),
                Highlights = fm.List("highlights"),
                Tags = await ResolveTagsAsync(fm.List("tags"), ct),
            });
            n++;
        }
        if (n > 0) await db.SaveChangesAsync(ct);
        return n;
    }

    private async Task<int> ImportBioAsync(string root, CancellationToken ct)
    {
        var bioFile = Path.Combine(root, "bio.md");
        if (!File.Exists(bioFile)) return 0;
        if (await db.BioProfiles.IgnoreQueryFilters().AnyAsync(ct)) return 0;

        var (fm, body) = FrontMatter.Parse(await File.ReadAllTextAsync(bioFile, ct));
        var profile = new BioProfile
        {
            DisplayName = fm.Str("displayName") ?? "",
            Headline = fm.Str("headline"),
            AvatarUrl = fm.Str("avatarUrl"),
            ContactEmail = fm.Str("contactEmail"),
            Location = fm.Str("location"),
            WebsiteUrl = fm.Str("websiteUrl"),
            GithubUsername = fm.Str("githubUsername"),
            SummaryMarkdown = body,
            Links = fm.Links,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var entriesFile = Path.Combine(root, "bio-entries.json");
        if (File.Exists(entriesFile))
        {
            var json = await File.ReadAllTextAsync(entriesFile, ct);
            var dtos = JsonSerializer.Deserialize<List<BioEntryDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            var order = 0;
            foreach (var d in dtos)
            {
                profile.Entries.Add(new BioEntry
                {
                    Kind = Enum.TryParse<BioEntryKind>(d.Kind, true, out var k) ? k : BioEntryKind.Experience,
                    Title = d.Title ?? "",
                    Organization = d.Organization,
                    Location = d.Location,
                    StartDate = ParseDate(d.StartDate) ?? new DateOnly(2020, 1, 1),
                    EndDate = ParseDate(d.EndDate),
                    IsCurrent = d.IsCurrent,
                    DescriptionMarkdown = d.Description,
                    SortOrder = order++,
                });
            }
        }

        db.BioProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return 1;
    }

    private async Task<int> ImportResumeAsync(string root, CancellationToken ct)
    {
        var file = Path.Combine(root, "resume.json");
        if (!File.Exists(file)) return 0;
        if (await db.Skills.IgnoreQueryFilters().AnyAsync(ct)
            || await db.Languages.IgnoreQueryFilters().AnyAsync(ct)
            || await db.Testimonials.IgnoreQueryFilters().AnyAsync(ct)) return 0;

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dto = JsonSerializer.Deserialize<ResumeDto>(await File.ReadAllTextAsync(file, ct), opts);
        if (dto is null) return 0;

        var order = 0;
        foreach (var s in dto.Skills ?? [])
            db.Skills.Add(new Skill { Name = s.Name ?? "", Category = s.Category, Level = s.Level, LevelLabel = s.LevelLabel, SortOrder = order++ });
        order = 0;
        foreach (var l in dto.Languages ?? [])
            db.Languages.Add(new Language { Name = l.Name ?? "", Level = l.Level, Stars = l.Stars, SortOrder = order++ });
        order = 0;
        foreach (var t in dto.Testimonials ?? [])
            db.Testimonials.Add(new Testimonial { Quote = t.Quote ?? "", AuthorName = t.AuthorName ?? "", AuthorTitle = t.AuthorTitle, SortOrder = order++ });
        order = 0;
        foreach (var m in dto.Music ?? [])
            db.ResumeListItems.Add(new ResumeListItem { ListKey = ResumeLists.Music, Label = m.Label ?? "", Url = m.Url, SortOrder = order++ });
        order = 0;
        foreach (var c in dto.Conferences ?? [])
            db.ResumeListItems.Add(new ResumeListItem { ListKey = ResumeLists.Conferences, Label = c.Label ?? "", Url = c.Url, Note = c.Note, SortOrder = order++ });

        await db.SaveChangesAsync(ct);
        return 1;
    }

    private async Task<List<Category>> ResolveCategoriesAsync(List<string> names, CancellationToken ct)
    {
        var result = new List<Category>();
        foreach (var name in names.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct())
        {
            var slug = Services.Slug.From(name);
            // Revisa primero el change tracker (entidades creadas en esta misma corrida, aún sin guardar).
            var cat = db.Categories.Local.FirstOrDefault(c => c.Slug == slug)
                ?? await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct)
                ?? new Category { Name = name, Slug = slug };
            result.Add(cat);
        }
        return result;
    }

    private async Task<List<Tag>> ResolveTagsAsync(List<string> names, CancellationToken ct)
    {
        var result = new List<Tag>();
        foreach (var name in names.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct())
        {
            var slug = Services.Slug.From(name);
            var tag = db.Tags.Local.FirstOrDefault(t => t.Slug == slug)
                ?? await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, ct)
                ?? new Tag { Name = name, Slug = slug };
            result.Add(tag);
        }
        return result;
    }

    private static DateOnly? ParseDate(string? value)
        => DateOnly.TryParse(value, out var d) ? d : null;

    private sealed class BioEntryDto
    {
        public string? Kind { get; set; }
        public string? Title { get; set; }
        public string? Organization { get; set; }
        public string? Location { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    private sealed class ResumeDto
    {
        public List<SkillDto>? Skills { get; set; }
        public List<LanguageDto>? Languages { get; set; }
        public List<TestimonialDto>? Testimonials { get; set; }
        public List<ListItemDto>? Music { get; set; }
        public List<ListItemDto>? Conferences { get; set; }
    }

    private sealed class SkillDto { public string? Name { get; set; } public string? Category { get; set; } public int Level { get; set; } public string? LevelLabel { get; set; } }
    private sealed class LanguageDto { public string? Name { get; set; } public string? Level { get; set; } public int Stars { get; set; } }
    private sealed class TestimonialDto { public string? Quote { get; set; } public string? AuthorName { get; set; } public string? AuthorTitle { get; set; } }
    private sealed class ListItemDto { public string? Label { get; set; } public string? Url { get; set; } public string? Note { get; set; } }
}
