using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin;

public sealed class BioModel(IBioService bio) : PageModel
{
    [BindProperty] public string DisplayName { get; set; } = string.Empty;
    [BindProperty] public string? Headline { get; set; }
    [BindProperty] public string? AvatarUrl { get; set; }
    [BindProperty] public string? ContactEmail { get; set; }
    [BindProperty] public string SummaryMarkdown { get; set; } = string.Empty;

    public IReadOnlyList<BioEntry> Entries { get; private set; } = [];

    // Alta rápida de un hito.
    [BindProperty] public BioEntryKind NewKind { get; set; }
    [BindProperty] public string? NewTitle { get; set; }
    [BindProperty] public string? NewOrganization { get; set; }
    [BindProperty] public DateOnly NewStartDate { get; set; }
    [BindProperty] public DateOnly? NewEndDate { get; set; }
    [BindProperty] public bool NewIsCurrent { get; set; }

    public IEnumerable<BioEntryKind> Kinds => Enum.GetValues<BioEntryKind>();

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveProfileAsync(CancellationToken ct)
    {
        var profile = await bio.GetOrCreateProfileAsync(ct);
        profile.DisplayName = DisplayName;
        profile.Headline = Headline;
        profile.AvatarUrl = AvatarUrl;
        profile.ContactEmail = ContactEmail;
        profile.SummaryMarkdown = SummaryMarkdown;
        await bio.SaveProfileAsync(profile, ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddEntryAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var profile = await bio.GetOrCreateProfileAsync(ct);
            await bio.SaveEntryAsync(new BioEntry
            {
                BioProfileId = profile.Id,
                Kind = NewKind,
                Title = NewTitle!,
                Organization = NewOrganization,
                StartDate = NewStartDate,
                EndDate = NewIsCurrent ? null : NewEndDate,
                IsCurrent = NewIsCurrent,
            }, ct);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteEntryAsync(int entryId, CancellationToken ct)
    {
        await bio.DeleteEntryAsync(entryId, ct);
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        var profile = await bio.GetOrCreateProfileAsync(ct);
        DisplayName = profile.DisplayName;
        Headline = profile.Headline;
        AvatarUrl = profile.AvatarUrl;
        ContactEmail = profile.ContactEmail;
        SummaryMarkdown = profile.SummaryMarkdown;
        Entries = profile.Entries.OrderByDescending(e => e.StartDate).ToList();
    }
}
