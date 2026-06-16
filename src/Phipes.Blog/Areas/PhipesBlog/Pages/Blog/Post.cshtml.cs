using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Blog;

public sealed class PostModel(IBlogService blog, ICommentService comments) : PageModel
{
    public PostDetail Post { get; private set; } = default!;
    public IReadOnlyList<Comment> Comments { get; private set; } = [];

    [BindProperty] public string CommentAuthor { get; set; } = string.Empty;
    [BindProperty] public string? CommentEmail { get; set; }
    [BindProperty] public string CommentBody { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        var post = await blog.GetPublishedBySlugAsync(slug, ct);
        if (post is null) return NotFound();
        Post = post;
        Comments = await comments.GetApprovedAsync(post.Id, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug, CancellationToken ct)
    {
        var post = await blog.GetPublishedBySlugAsync(slug, ct);
        if (post is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(CommentBody) && !string.IsNullOrWhiteSpace(CommentAuthor))
        {
            await comments.AddAsync(new Comment
            {
                BlogPostId = post.Id,
                AuthorName = CommentAuthor,
                AuthorEmail = CommentEmail,
                Body = CommentBody,
            }, ct);
            TempData["CommentSubmitted"] = true;
        }
        return RedirectToPage(new { slug });
    }
}
