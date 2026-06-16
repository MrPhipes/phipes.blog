using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Implementación EF Core de <see cref="IProjectService"/>.</summary>
public sealed class EfProjectService(
    IPhipesBlogDbContext db,
    IMarkdownRenderer markdown,
    IBlogUserContext user,
    IOptions<PhipesBlogOptions> options) : IProjectService
{
    private readonly PhipesBlogOptions _options = options.Value;

    public async Task<PagedResult<ProjectListItem>> GetPublishedAsync(int page = 1, int? pageSize = null, string? tagSlug = null, CancellationToken ct = default)
    {
        var size = pageSize ?? _options.PageSize;
        page = Math.Max(1, page);

        var query = db.Projects.AsNoTracking().Where(p => p.Status == ContentStatus.Published);
        if (!string.IsNullOrEmpty(tagSlug))
            query = query.Where(p => p.Tags.Any(t => t.Slug == tagSlug));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.PublishedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(ct);
        return new PagedResult<ProjectListItem>(items.Select(ToListItem).ToList(), page, size, total);
    }

    public async Task<ProjectDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        var p = await db.Projects.AsNoTracking().Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.Status == ContentStatus.Published, ct);
        return p is null ? null : ToDetail(p);
    }

    public async Task<IReadOnlyList<ProjectListItem>> GetFeaturedAsync(int count = 3, CancellationToken ct = default)
    {
        var items = await db.Projects.AsNoTracking()
            .Where(p => p.Status == ContentStatus.Published && p.IsFeatured)
            .OrderByDescending(p => p.PublishedAt).Take(count).ToListAsync(ct);
        return items.Select(ToListItem).ToList();
    }

    public async Task<PagedResult<ProjectListItem>> ListAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        var total = await db.Projects.CountAsync(ct);
        var items = await db.Projects.AsNoTracking()
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ProjectListItem>(items.Select(ToListItem).ToList(), page, pageSize, total);
    }

    public async Task<ProjectEditModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var p = await db.Projects.AsNoTracking().Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return null;
        return new ProjectEditModel
        {
            Id = p.Id, Title = p.Title, Slug = p.Slug, Summary = p.Summary, BodyMarkdown = p.BodyMarkdown,
            CoverImageUrl = p.CoverImageUrl, ClientName = p.ClientName, Role = p.Role, Outcome = p.Outcome,
            ProjectUrl = p.ProjectUrl, StartedOn = p.StartedOn, CompletedOn = p.CompletedOn,
            IsFeatured = p.IsFeatured, Status = p.Status,
            TechStack = p.TechStack.ToList(), Highlights = p.Highlights.ToList(),
            Tags = p.Tags.Select(t => t.Name).ToList(),
        };
    }

    public async Task<int> SaveAsync(ProjectEditModel model, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        Project project;
        if (model.Id is { } id)
        {
            project = await db.Projects.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id, ct)
                ?? throw new InvalidOperationException($"Proyecto {id} no encontrado.");
        }
        else
        {
            project = new Project { CreatedAt = now, AuthorUserId = user.UserId ?? "", AuthorDisplayName = user.DisplayName ?? "" };
            db.Projects.Add(project);
        }

        project.Title = model.Title;
        project.Slug = string.IsNullOrWhiteSpace(model.Slug) ? Slug.From(model.Title) : Slug.From(model.Slug);
        project.Summary = string.IsNullOrWhiteSpace(model.Summary) ? markdown.ToPlainText(model.BodyMarkdown) : model.Summary;
        project.BodyMarkdown = model.BodyMarkdown;
        project.CoverImageUrl = model.CoverImageUrl;
        project.ClientName = model.ClientName;
        project.Role = model.Role;
        project.Outcome = model.Outcome;
        project.ProjectUrl = model.ProjectUrl;
        project.StartedOn = model.StartedOn;
        project.CompletedOn = model.CompletedOn;
        project.IsFeatured = model.IsFeatured;
        project.TechStack = model.TechStack.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        project.Highlights = model.Highlights.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        project.UpdatedAt = now;

        if (project.Status != ContentStatus.Published && model.Status == ContentStatus.Published)
            project.PublishedAt = now;
        project.Status = model.Status;

        project.Tags.Clear();
        foreach (var name in model.Tags.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
        {
            var slug = Slug.From(name);
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, ct) ?? new Tag { Name = name, Slug = slug };
            project.Tags.Add(tag);
        }

        await db.SaveChangesAsync(ct);
        return project.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var p = await db.Projects.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return;
        db.Projects.Remove(p);
        await db.SaveChangesAsync(ct);
    }

    private static ProjectListItem ToListItem(Project p) => new(
        p.Id, p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.Outcome, p.ClientName, p.TechStack.ToList(), p.IsFeatured);

    private ProjectDetail ToDetail(Project p) => new(
        p.Id, p.Slug, p.Title, p.Summary, markdown.ToHtml(p.BodyMarkdown), p.CoverImageUrl,
        p.ClientName, p.Role, p.Outcome, p.ProjectUrl, p.StartedOn, p.CompletedOn,
        p.TechStack.ToList(), p.Highlights.ToList(), p.Tags.Select(t => t.Name).ToList());
}
