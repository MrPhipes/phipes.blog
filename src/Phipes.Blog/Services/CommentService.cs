using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Alta y moderación de comentarios.</summary>
public interface ICommentService
{
    Task<IReadOnlyList<Comment>> GetApprovedAsync(int postId, CancellationToken ct = default);
    Task<int> AddAsync(Comment comment, CancellationToken ct = default);
    Task<IReadOnlyList<Comment>> GetPendingAsync(CancellationToken ct = default);
    Task SetStatusAsync(int id, CommentStatus status, CancellationToken ct = default);
}

/// <summary>Implementación EF Core. Respeta <see cref="PhipesBlogOptions.ModerateComments"/>.</summary>
public sealed class EfCommentService(
    PhipesBlogDbContext db,
    IBlogUserContext user,
    IOptions<PhipesBlogOptions> options) : ICommentService
{
    private readonly PhipesBlogOptions _options = options.Value;

    public async Task<IReadOnlyList<Comment>> GetApprovedAsync(int postId, CancellationToken ct = default)
        => await db.Comments.AsNoTracking()
            .Where(c => c.BlogPostId == postId && c.Status == CommentStatus.Approved)
            .OrderBy(c => c.CreatedAt).ToListAsync(ct);

    public async Task<int> AddAsync(Comment comment, CancellationToken ct = default)
    {
        comment.CreatedAt = DateTimeOffset.UtcNow;
        comment.Status = _options.ModerateComments ? CommentStatus.Pending : CommentStatus.Approved;
        if (user.IsAuthenticated)
        {
            comment.AuthorUserId = user.UserId;
            if (string.IsNullOrWhiteSpace(comment.AuthorName))
                comment.AuthorName = user.DisplayName ?? "Usuario";
        }
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);
        return comment.Id;
    }

    public async Task<IReadOnlyList<Comment>> GetPendingAsync(CancellationToken ct = default)
        => await db.Comments.AsNoTracking()
            .Where(c => c.Status == CommentStatus.Pending)
            .OrderBy(c => c.CreatedAt).ToListAsync(ct);

    public async Task SetStatusAsync(int id, CommentStatus status, CancellationToken ct = default)
    {
        var c = await db.Comments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return;
        c.Status = status;
        await db.SaveChangesAsync(ct);
    }
}
