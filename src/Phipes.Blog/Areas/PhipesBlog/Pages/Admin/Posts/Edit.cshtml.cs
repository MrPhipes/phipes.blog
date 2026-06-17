using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Domain;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin.Posts;

public sealed class EditModel(IBlogService blog, IImageStorage images) : PageModel
{
    [BindProperty] public PostEditModel Input { get; set; } = new();
    [BindProperty] public IFormFile? CoverFile { get; set; }
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

        if (CoverFile is { Length: > 0 })
        {
            await using var stream = CoverFile.OpenReadStream();
            Input.CoverImageUrl = await images.SaveAsync(stream, CoverFile.FileName, ct);
        }

        await blog.SaveAsync(Input, ct);
        return Redirect("/admin/posts");
    }

    private static List<string> Split(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? new()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    public IEnumerable<ContentStatus> Statuses => Enum.GetValues<ContentStatus>();
}
