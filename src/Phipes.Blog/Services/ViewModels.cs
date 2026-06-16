using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Resultado paginado genérico para listados.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>Item de listado de blog (sin cuerpo).</summary>
public sealed record PostListItem(
    int Id, string Slug, string Title, string Summary, string? CoverImageUrl,
    DateTimeOffset? PublishedAt, string AuthorDisplayName, int ReadingMinutes,
    IReadOnlyList<string> Categories, IReadOnlyList<string> Tags);

/// <summary>Detalle de un post con el cuerpo ya renderizado a HTML.</summary>
public sealed record PostDetail(
    int Id, string Slug, string Title, string Summary, string BodyHtml, string? CoverImageUrl,
    DateTimeOffset? PublishedAt, string AuthorDisplayName, int ReadingMinutes,
    IReadOnlyList<string> Categories, IReadOnlyList<string> Tags);

/// <summary>Item de listado de proyectos (enfoque comercial).</summary>
public sealed record ProjectListItem(
    int Id, string Slug, string Title, string Summary, string? CoverImageUrl,
    string? Outcome, string? ClientName, IReadOnlyList<string> TechStack, bool IsFeatured);

/// <summary>Detalle de un proyecto con el cuerpo renderizado.</summary>
public sealed record ProjectDetail(
    int Id, string Slug, string Title, string Summary, string BodyHtml, string? CoverImageUrl,
    string? ClientName, string? Role, string? Outcome, string? ProjectUrl,
    DateOnly? StartedOn, DateOnly? CompletedOn,
    IReadOnlyList<string> TechStack, IReadOnlyList<string> Highlights, IReadOnlyList<string> Tags);

/// <summary>Vista de la biografía: perfil + hitos agrupados, con resumen renderizado.</summary>
public sealed record BioView(
    string DisplayName, string? Headline, string? AvatarUrl, string SummaryHtml,
    string? ContactEmail, string? Location, string? WebsiteUrl, string? GithubUsername,
    IReadOnlyList<BioLink> Links, IReadOnlyList<BioEntryView> Entries);

/// <summary>Hito de la biografía con descripción renderizada.</summary>
public sealed record BioEntryView(
    BioEntryKind Kind, string Title, string? Organization, string? Location,
    DateOnly StartDate, DateOnly? EndDate, bool IsCurrent, string? DescriptionHtml);

/// <summary>Datos que el admin envía al crear/editar un post.</summary>
public sealed class PostEditModel
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string BodyMarkdown { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>Datos que el admin envía al crear/editar un proyecto.</summary>
public sealed class ProjectEditModel
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string BodyMarkdown { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? ClientName { get; set; }
    public string? Role { get; set; }
    public string? Outcome { get; set; }
    public string? ProjectUrl { get; set; }
    public DateOnly? StartedOn { get; set; }
    public DateOnly? CompletedOn { get; set; }
    public bool IsFeatured { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public List<string> TechStack { get; set; } = new();
    public List<string> Highlights { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}
