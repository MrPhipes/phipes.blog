using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Blog;

public sealed class IndexModel(IBlogService blog) : PageModel
{
    public PagedResult<PostListItem> Posts { get; private set; } = default!;

    [BindProperty(SupportsGet = true, Name = "page")] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Tag { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
        => Posts = await blog.GetPublishedAsync(PageNumber, categorySlug: Category, tagSlug: Tag, ct: ct);
}
