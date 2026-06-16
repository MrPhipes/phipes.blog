using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>Comentario sobre una entrada de blog. Soporta respuestas anidadas y moderación.</summary>
public sealed class Comment
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    public int BlogPostId { get; set; }
    public BlogPost? BlogPost { get; set; }

    /// <summary>Comentario padre, para hilos. Null = comentario raíz.</summary>
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }

    /// <summary>Id de usuario si el autor estaba autenticado; null si fue anónimo.</summary>
    [MaxLength(450)]
    public string? AuthorUserId { get; set; }

    [Required, MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [MaxLength(320)]
    public string? AuthorEmail { get; set; }

    [Required, MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public CommentStatus Status { get; set; } = CommentStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; }
}
