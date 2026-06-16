namespace Phipes.Blog.Domain;

/// <summary>Entrada de blog: artículo técnico o personal.</summary>
public sealed class BlogPost : ContentEntity
{
    /// <summary>Minutos estimados de lectura (0 = calcular en lectura).</summary>
    public int ReadingMinutes { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
