using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>Habilidad/competencia con nivel, para la sección "Skills" (barras de nivel).</summary>
public sealed class Skill
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Categoría opcional para agrupar (ej. "Backend", "Datos").</summary>
    [MaxLength(80)]
    public string? Category { get; set; }

    /// <summary>Nivel 0–100 (ancho de la barra).</summary>
    [Range(0, 100)]
    public int Level { get; set; }

    /// <summary>Etiqueta del nivel (ej. "Experto", "Avanzado").</summary>
    [MaxLength(40)]
    public string? LevelLabel { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>Testimonio de un cliente o colega.</summary>
public sealed class Testimonial
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Quote { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Cargo/empresa del autor (ej. "Gerente de Operaciones, DIMACO").</summary>
    [MaxLength(160)]
    public string? AuthorTitle { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>Idioma con nivel y puntuación en estrellas (0–5).</summary>
public sealed class Language
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Etiqueta de nivel (ej. "Nativo", "Profesional").</summary>
    [MaxLength(80)]
    public string? Level { get; set; }

    /// <summary>Estrellas 0–5 (se permite media con .5 → guardamos x10 para enteros si hiciera falta; aquí entero).</summary>
    [Range(0, 5)]
    public int Stars { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>
/// Ítem de una lista simple del currículum (música para programar, conferencias, etc.).
/// Un solo tipo cubre varias listas mediante <see cref="ListKey"/>.
/// </summary>
public sealed class ResumeListItem
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Clave de la lista a la que pertenece: "music", "conferences", etc.</summary>
    [Required, MaxLength(40)]
    public string ListKey { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Url { get; set; }

    /// <summary>Nota o detalle (ej. la ciudad de una conferencia).</summary>
    [MaxLength(200)]
    public string? Note { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>Claves canónicas de las listas de currículum incluidas.</summary>
public static class ResumeLists
{
    public const string Music = "music";
    public const string Conferences = "conferences";
}
