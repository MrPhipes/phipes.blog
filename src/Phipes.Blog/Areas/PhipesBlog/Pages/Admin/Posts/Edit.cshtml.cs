using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin.Posts;

public sealed class EditModel(IBlogService blog) : PageModel
{
    [BindProperty] public PostEditModel Input { get; set; } = new();
    [BindProperty] public string? CategoriesCsv { get; set; }
    [BindProperty] public string? TagsCsv { get; set; }

    public bool IsNew => Input.Id is null;

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        if (id is { } existing)
        {
            var model = await blog.GetForEditAsync(existing, ct);
            if (model is null) return NotFound();
            Input = model;
            CategoriesCsv = string.Join(", ", model.Categories);
            TagsCsv = string.Join(", ", model.Tags);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Input.Categories = Split(CategoriesCsv);
        Input.Tags = Split(TagsCsv);

        if (string.IsNullOrWhiteSpace(Input.Title))
            ModelState.AddModelError("Input.Title", "El título es obligatorio.");
        if (!ModelState.IsValid) return Page();

        await blog.SaveAsync(Input, ct);
        return RedirectToPage("Index");
    }

    private static List<string> Split(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? new()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    public IEnumerable<ContentStatus> Statuses => Enum.GetValues<ContentStatus>();
}
