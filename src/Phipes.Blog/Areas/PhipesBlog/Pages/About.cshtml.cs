using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages;

public sealed class AboutModel(IBioService bio) : PageModel
{
    public BioView? Bio { get; private set; }

    public async Task OnGetAsync(CancellationToken ct) => Bio = await bio.GetAsync(ct);
}
