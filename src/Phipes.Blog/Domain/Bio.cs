using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>
/// Perfil biográfico del dueño del sitio (uno por tenant). Alimenta la página "sobre mí":
/// titular, foto, resumen y enlaces, más una línea de tiempo de hitos (<see cref="BioEntry"/>).
/// </summary>
public sealed class BioProfile
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Titular profesional (ej. "Ingeniero de software · Integraciones .NET / SAP").</summary>
    [MaxLength(300)]
    public string? Headline { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>Presentación en Markdown.</summary>
    public string SummaryMarkdown { get; set; } = string.Empty;

    /// <summary>Correo de contacto público (opcional).</summary>
    [MaxLength(320)]
    public string? ContactEmail { get; set; }

    /// <summary>Ubicación mostrada en la ficha (ej. "Talca, Chile").</summary>
    [MaxLength(160)]
    public string? Location { get; set; }

    /// <summary>Latitud para el mini mapa (opcional).</summary>
    public double? Latitude { get; set; }

    /// <summary>Longitud para el mini mapa (opcional).</summary>
    public double? Longitude { get; set; }

    /// <summary>Sitio web personal mostrado en la ficha.</summary>
    [MaxLength(300)]
    public string? WebsiteUrl { get; set; }

    /// <summary>Usuario de GitHub para el calendario de contribuciones (ej. "MrPhipes").</summary>
    [MaxLength(100)]
    public string? GithubUsername { get; set; }

    /// <summary>Enlaces sociales/profesionales (LinkedIn, GitHub, etc.). Persistido como JSON.</summary>
    public List<BioLink> Links { get; set; } = new();

    public ICollection<BioEntry> Entries { get; set; } = new List<BioEntry>();

    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>Enlace externo del perfil (label + url + icono opcional).</summary>
public sealed class BioLink
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    /// <summary>Clase de icono (ej. "fab fa-linkedin"), opcional y específica del tema.</summary>
    public string? Icon { get; set; }
}

/// <summary>Hito de la línea de tiempo profesional (experiencia, educación, certificación, logro).</summary>
public sealed class BioEntry
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    public int BioProfileId { get; set; }
    public BioProfile? BioProfile { get; set; }

    public BioEntryKind Kind { get; set; }

    [Required, MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Organización / institución (empresa, universidad, etc.).</summary>
    [MaxLength(240)]
    public string? Organization { get; set; }

    [MaxLength(160)]
    public string? Location { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>True si es el cargo/estudio actual (sin fecha de término).</summary>
    public bool IsCurrent { get; set; }

    /// <summary>Detalle en Markdown.</summary>
    public string? DescriptionMarkdown { get; set; }

    /// <summary>Orden manual dentro de su grupo (menor primero).</summary>
    public int SortOrder { get; set; }
}
