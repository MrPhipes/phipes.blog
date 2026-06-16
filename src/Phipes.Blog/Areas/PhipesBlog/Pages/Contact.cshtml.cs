using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages;

public sealed class ContactModel(IContactService contact) : PageModel
{
    [BindProperty, Required] public string FromName { get; set; } = string.Empty;
    [BindProperty, Required, EmailAddress] public string FromEmail { get; set; } = string.Empty;
    [BindProperty] public string? Subject { get; set; }
    [BindProperty, Required] public string Body { get; set; } = string.Empty;

    public bool Sent { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        await contact.SubmitAsync(new ContactMessage
        {
            FromName = FromName,
            FromEmail = FromEmail,
            Subject = Subject,
            Body = Body,
        }, ct);

        Sent = true;
        ModelState.Clear();
        FromName = FromEmail = Body = string.Empty;
        Subject = null;
        return Page();
    }
}
