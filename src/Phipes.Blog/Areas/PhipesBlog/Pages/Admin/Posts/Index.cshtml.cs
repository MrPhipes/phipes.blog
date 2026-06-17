using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin.Posts;

public sealed class IndexModel(IBlogService blog) : PageModel
{
    public PagedResult<PostListItem> Posts { get; private set; } = default!;
    [BindProperty(SupportsGet = true, Name = "page")] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct)
        => Posts = await blog.ListAllAsync(PageNumber, 20, ct);

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        await blog.DeleteAsync(id, ct);
        return Redirect("/admin/posts");
    }
}
