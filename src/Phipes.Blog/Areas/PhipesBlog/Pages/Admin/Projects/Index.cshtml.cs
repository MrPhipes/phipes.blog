using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin.Projects;

public sealed class IndexModel(IProjectService projects) : PageModel
{
    public PagedResult<ProjectListItem> Projects { get; private set; } = default!;
    [BindProperty(SupportsGet = true, Name = "page")] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct)
        => Projects = await projects.ListAllAsync(PageNumber, 20, ct);

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        await projects.DeleteAsync(id, ct);
        return Redirect("/admin/projects");
    }
}
