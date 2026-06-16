using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin;

public sealed class ResumeModel(IResumeService resume) : PageModel
{
    public IReadOnlyList<Skill> Skills { get; private set; } = [];
    public IReadOnlyList<Testimonial> Testimonials { get; private set; } = [];
    public IReadOnlyList<Language> Languages { get; private set; } = [];
    public IReadOnlyList<ResumeListItem> Music { get; private set; } = [];
    public IReadOnlyList<ResumeListItem> Conferences { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    // ---- Skills ----
    public async Task<IActionResult> OnPostAddSkillAsync(string name, string? category, int level, string? levelLabel, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(name))
            await resume.SaveSkillAsync(new Skill { Name = name, Category = category, Level = Math.Clamp(level, 0, 100), LevelLabel = levelLabel }, ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteSkillAsync(int id, CancellationToken ct)
    {
        await resume.DeleteSkillAsync(id, ct);
        return RedirectToPage();
    }

    // ---- Testimonials ----
    public async Task<IActionResult> OnPostAddTestimonialAsync(string quote, string authorName, string? authorTitle, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(quote) && !string.IsNullOrWhiteSpace(authorName))
            await resume.SaveTestimonialAsync(new Testimonial { Quote = quote, AuthorName = authorName, AuthorTitle = authorTitle }, ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteTestimonialAsync(int id, CancellationToken ct)
    {
        await resume.DeleteTestimonialAsync(id, ct);
        return RedirectToPage();
    }

    // ---- Languages ----
    public async Task<IActionResult> OnPostAddLanguageAsync(string name, string? level, int stars, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(name))
            await resume.SaveLanguageAsync(new Language { Name = name, Level = level, Stars = Math.Clamp(stars, 0, 5) }, ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteLanguageAsync(int id, CancellationToken ct)
    {
        await resume.DeleteLanguageAsync(id, ct);
        return RedirectToPage();
    }

    // ---- Listas (música / conferencias) ----
    public async Task<IActionResult> OnPostAddListItemAsync(string listKey, string label, string? url, string? note, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(label))
            await resume.SaveListItemAsync(new ResumeListItem { ListKey = listKey, Label = label, Url = url, Note = note }, ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteListItemAsync(int id, CancellationToken ct)
    {
        await resume.DeleteListItemAsync(id, ct);
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Skills = await resume.GetSkillsAsync(ct);
        Testimonials = await resume.GetTestimonialsAsync(ct);
        Languages = await resume.GetLanguagesAsync(ct);
        Music = await resume.GetListAsync(ResumeLists.Music, ct);
        Conferences = await resume.GetListAsync(ResumeLists.Conferences, ct);
    }
}
