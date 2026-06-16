using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>Categoría de contenido (por tenant). Agrupa entradas de blog.</summary>
public sealed class Category
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}

/// <summary>Etiqueta de contenido (por tenant). Compartida por posts y proyectos.</summary>
public sealed class Tag
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
