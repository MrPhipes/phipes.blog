using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Implementación EF Core de <see cref="IBlogService"/>.</summary>
public sealed class EfBlogService(
    PhipesBlogDbContext db,
    IMarkdownRenderer markdown,
    IBlogUserContext user,
    IOptions<PhipesBlogOptions> options) : IBlogService
{
    private readonly PhipesBlogOptions _options = options.Value;

    public async Task<PagedResult<PostListItem>> GetPublishedAsync(
        int page = 1, int? pageSize = null, string? categorySlug = null, string? tagSlug = null,
        CancellationToken ct = default)
    {
        var size = pageSize ?? _options.PageSize;
        page = Math.Max(1, page);

        var query = db.Posts
            .AsNoTracking()
            .Where(p => p.Status == ContentStatus.Published);

        if (!string.IsNullOrEmpty(categorySlug))
            query = query.Where(p => p.Categories.Any(c => c.Slug == categorySlug));
        if (!string.IsNullOrEmpty(tagSlug))
            query = query.Where(p => p.Tags.Any(t => t.Slug == tagSlug));

        var total = await query.CountAsync(ct);
        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * size).Take(size)
            .Include(p => p.Categories).Include(p => p.Tags)
            .ToListAsync(ct);

        return new PagedResult<PostListItem>(posts.Select(ToListItem).ToList(), page, size, total);
    }

    public async Task<PostDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await db.Posts.AsNoTracking()
            .Include(p => p.Categories).Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published, ct);
        return post is null ? null : ToDetail(post);
    }

    public async Task<IReadOnlyList<PostListItem>> GetFeaturedAsync(int count = 3, CancellationToken ct = default)
    {
        var posts = await db.Posts.AsNoTracking()
            .Where(p => p.Status == ContentStatus.Published && p.IsFeatured)
            .OrderByDescending(p => p.PublishedAt).Take(count)
            .Include(p => p.Categories).Include(p => p.Tags)
            .ToListAsync(ct);
        return posts.Select(ToListItem).ToList();
    }

    public async Task<PagedResult<PostListItem>> ListAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        var total = await db.Posts.CountAsync(ct);
        var posts = await db.Posts.AsNoTracking()
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Include(p => p.Categories).Include(p => p.Tags)
            .ToListAsync(ct);
        return new PagedResult<PostListItem>(posts.Select(ToListItem).ToList(), page, pageSize, total);
    }

    public async Task<PostEditModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var p = await db.Posts.AsNoTracking()
            .Include(x => x.Categories).Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return null;
        return new PostEditModel
        {
            Id = p.Id, Title = p.Title, Slug = p.Slug, Summary = p.Summary,
            BodyMarkdown = p.BodyMarkdown, CoverImageUrl = p.CoverImageUrl,
            IsFeatured = p.IsFeatured, Status = p.Status,
            Categories = p.Categories.Select(c => c.Name).ToList(),
            Tags = p.Tags.Select(t => t.Name).ToList(),
        };
    }

    public async Task<int> SaveAsync(PostEditModel model, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        BlogPost post;
        if (model.Id is { } id)
        {
            post = await db.Posts.Include(p => p.Categories).Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id, ct)
                ?? throw new InvalidOperationException($"Post {id} no encontrado.");
        }
        else
        {
            post = new BlogPost { CreatedAt = now, AuthorUserId = user.UserId ?? "", AuthorDisplayName = user.DisplayName ?? "" };
            db.Posts.Add(post);
        }

        post.Title = model.Title;
        post.Slug = string.IsNullOrWhiteSpace(model.Slug) ? Slug.From(model.Title) : Slug.From(model.Slug);
        post.Summary = string.IsNullOrWhiteSpace(model.Summary) ? markdown.ToPlainText(model.BodyMarkdown) : model.Summary;
        post.BodyMarkdown = model.BodyMarkdown;
        post.CoverImageUrl = model.CoverImageUrl;
        post.IsFeatured = model.IsFeatured;
        post.ReadingMinutes = EstimateReadingMinutes(model.BodyMarkdown);
        post.UpdatedAt = now;

        if (post.Status != ContentStatus.Published && model.Status == ContentStatus.Published)
            post.PublishedAt = now;
        post.Status = model.Status;

        await SyncCategoriesAsync(post, model.Categories, ct);
        await SyncTagsAsync(post, model.Tags, ct);

        await db.SaveChangesAsync(ct);
        return post.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post is null) return;
        db.Posts.Remove(post);
        await db.SaveChangesAsync(ct);
    }

    private async Task SyncCategoriesAsync(BlogPost post, List<string> names, CancellationToken ct)
    {
        post.Categories.Clear();
        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
        {
            var slug = Slug.From(name);
            var cat = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct)
                ?? new Category { Name = name, Slug = slug };
            post.Categories.Add(cat);
        }
    }

    private async Task SyncTagsAsync(BlogPost post, List<string> names, CancellationToken ct)
    {
        post.Tags.Clear();
        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
        {
            var slug = Slug.From(name);
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, ct)
                ?? new Tag { Name = name, Slug = slug };
            post.Tags.Add(tag);
        }
    }

    private static int EstimateReadingMinutes(string markdown)
    {
        var words = markdown.Split(' ', '\n', '\r', '\t').Count(s => s.Length > 0);
        return Math.Max(1, (int)Math.Ceiling(words / 200.0));
    }

    private static PostListItem ToListItem(BlogPost p) => new(
        p.Id, p.Slug, p.Title, p.Summary, p.CoverImageUrl, p.PublishedAt, p.AuthorDisplayName,
        p.ReadingMinutes, p.Categories.Select(c => c.Name).ToList(), p.Tags.Select(t => t.Name).ToList());

    private PostDetail ToDetail(BlogPost p) => new(
        p.Id, p.Slug, p.Title, p.Summary, markdown.ToHtml(p.BodyMarkdown), p.CoverImageUrl,
        p.PublishedAt, p.AuthorDisplayName, p.ReadingMinutes,
        p.Categories.Select(c => c.Name).ToList(), p.Tags.Select(t => t.Name).ToList());
}
