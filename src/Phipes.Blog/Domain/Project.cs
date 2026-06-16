using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>
/// Entrada tipo proyecto: similar a un post pero con enfoque <b>comercial / marketing</b>
/// (caso de éxito para vender al profesional). Hereda el contenido Markdown y agrega
/// campos de venta: cliente, rol, stack, resultados y llamada a la acción.
/// </summary>
public sealed class Project : ContentEntity
{
    /// <summary>Cliente o contexto del proyecto (puede ser confidencial → texto libre).</summary>
    [MaxLength(200)]
    public string? ClientName { get; set; }

    /// <summary>Rol desempeñado (ej. "Arquitecto e implementador").</summary>
    [MaxLength(200)]
    public string? Role { get; set; }

    /// <summary>Resumen del resultado/impacto en una línea (gancho comercial).</summary>
    [MaxLength(400)]
    public string? Outcome { get; set; }

    /// <summary>URL pública del proyecto/demo, si existe.</summary>
    [MaxLength(500)]
    public string? ProjectUrl { get; set; }

    public DateOnly? StartedOn { get; set; }
    public DateOnly? CompletedOn { get; set; }

    /// <summary>Tecnologías usadas (chips). Persistido como CSV.</summary>
    public List<string> TechStack { get; set; } = new();

    /// <summary>Bullets de venta destacados. Persistido como JSON.</summary>
    public List<string> Highlights { get; set; } = new();

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
