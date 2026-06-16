using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin;

public sealed class CommentsModel(ICommentService comments) : PageModel
{
    public IReadOnlyList<Comment> Pending { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct) => Pending = await comments.GetPendingAsync(ct);

    public async Task<IActionResult> OnPostAsync(int id, CommentStatus status, CancellationToken ct)
    {
        await comments.SetStatusAsync(id, status, ct);
        return RedirectToPage();
    }
}
