using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin;

public sealed class MessagesModel(IContactService contact) : PageModel
{
    public PagedResult<ContactMessage> Messages { get; private set; } = default!;
    [BindProperty(SupportsGet = true, Name = "page")] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct) => Messages = await contact.ListAsync(PageNumber, 20, ct);

    public async Task<IActionResult> OnPostAsync(int id, ContactMessageStatus status, CancellationToken ct)
    {
        await contact.SetStatusAsync(id, status, ct);
        return RedirectToPage(new { page = PageNumber });
    }
}
