using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin.Projects;

public sealed class EditModel(IProjectService projects) : PageModel
{
    [BindProperty] public ProjectEditModel Input { get; set; } = new();
    [BindProperty] public string? TechStackCsv { get; set; }
    [BindProperty] public string? HighlightsLines { get; set; }
    [BindProperty] public string? TagsCsv { get; set; }

    public bool IsNew => Input.Id is null;

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        if (id is { } existing)
        {
            var model = await projects.GetForEditAsync(existing, ct);
            if (model is null) return NotFound();
            Input = model;
            TechStackCsv = string.Join(", ", model.TechStack);
            HighlightsLines = string.Join('\n', model.Highlights);
            TagsCsv = string.Join(", ", model.Tags);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Input.TechStack = SplitCsv(TechStackCsv);
        Input.Tags = SplitCsv(TagsCsv);
        Input.Highlights = SplitLines(HighlightsLines);

        if (string.IsNullOrWhiteSpace(Input.Title))
            ModelState.AddModelError("Input.Title", "El título es obligatorio.");
        if (!ModelState.IsValid) return Page();

        await projects.SaveAsync(Input, ct);
        return RedirectToPage("Index");
    }

    private static List<string> SplitCsv(string? csv)
        => string.IsNullOrWhiteSpace(csv) ? new()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static List<string> SplitLines(string? text)
        => string.IsNullOrWhiteSpace(text) ? new()
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

    public IEnumerable<ContentStatus> Statuses => Enum.GetValues<ContentStatus>();
}
