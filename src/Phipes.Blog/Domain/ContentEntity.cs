using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>
/// Base común a las piezas de contenido (entradas de blog y proyectos). El cuerpo se
/// almacena en <b>Markdown</b> (<see cref="BodyMarkdown"/>); el HTML se renderiza en lectura.
/// Toda pieza pertenece a un tenant (<see cref="TenantId"/>) para que varios sitios
/// (phipes.cl, pacificdev.cl) compartan el mismo esquema sin pisarse.
/// </summary>
public abstract class ContentEntity
{
    public int Id { get; set; }

    /// <summary>Discriminador de tenant. Lo resuelve <c>IBlogTenantResolver</c> al escribir.</summary>
    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Slug único por tenant, usado en la URL.</summary>
    [Required, MaxLength(160)]
    public string Slug { get; set; } = string.Empty;

    [Required, MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Resumen/extracto en texto plano para listados y metadatos.</summary>
    [MaxLength(800)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>Cuerpo en Markdown (fuente editable).</summary>
    [Required]
    public string BodyMarkdown { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public bool IsFeatured { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>Id del usuario autor en el proveedor de identidad del host (string, agnóstico).</summary>
    [Required, MaxLength(450)]
    public string AuthorUserId { get; set; } = string.Empty;

    /// <summary>Nombre del autor capturado al escribir (evita acoplar el paquete a la tabla de usuarios).</summary>
    [MaxLength(200)]
    public string AuthorDisplayName { get; set; } = string.Empty;
}
