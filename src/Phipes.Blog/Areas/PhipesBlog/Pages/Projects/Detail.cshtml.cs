using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Projects;

public sealed class DetailModel(IProjectService projects) : PageModel
{
    public ProjectDetail Project { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        var p = await projects.GetPublishedBySlugAsync(slug, ct);
        if (p is null) return NotFound();
        Project = p;
        return Page();
    }
}
