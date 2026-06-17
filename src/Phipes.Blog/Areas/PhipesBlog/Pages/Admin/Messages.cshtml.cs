using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin;

public sealed class MessagesModel(IContactService contact) : PageModel
{
    public PagedResult<ContactMessage> Messages { get; private set; } = default!;
    [BindProperty(SupportsGet = true, Name = "page")] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true, Name = "f")] public string Filter { get; set; } = "new";

    public int CountNew { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Messages = await contact.ListAsync(PageNumber, 20, MapFilter(Filter), ct);
        CountNew = await contact.CountByStatusAsync(ContactMessageStatus.New, ct);
    }

    public async Task<IActionResult> OnPostAsync(int id, ContactMessageStatus status, string? f, CancellationToken ct)
    {
        await contact.SetStatusAsync(id, status, ct);
        return Redirect($"/admin/messages?f={f ?? "new"}");
    }

    private static ContactMessageStatus? MapFilter(string? f) => f switch
    {
        "read" => ContactMessageStatus.Read,
        "replied" => ContactMessageStatus.Replied,
        "archived" => ContactMessageStatus.Archived,
        "all" => null,
        _ => ContactMessageStatus.New,
    };

    public static (string Key, string Label)[] FilterTabs =>
    [
        ("new", "Nuevos"),
        ("read", "Leídos"),
        ("replied", "Respondidos"),
        ("archived", "Archivados"),
        ("all", "Todos"),
    ];
}
