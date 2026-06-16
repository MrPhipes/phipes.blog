using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Projects;

public sealed class IndexModel(IProjectService projects) : PageModel
{
    public PagedResult<ProjectListItem> Projects { get; private set; } = default!;

    [BindProperty(SupportsGet = true, Name = "page")] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Tag { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
        => Projects = await projects.GetPublishedAsync(PageNumber, tagSlug: Tag, ct: ct);
}
