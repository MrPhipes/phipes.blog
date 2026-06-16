using System.ComponentModel.DataAnnotations;

namespace Phipes.Blog.Domain;

/// <summary>Mensaje recibido por el formulario de contacto del sitio.</summary>
public sealed class ContactMessage
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string TenantId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FromName { get; set; } = string.Empty;

    [Required, MaxLength(320)]
    public string FromEmail { get; set; } = string.Empty;

    [MaxLength(240)]
    public string? Subject { get; set; }

    [Required, MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    /// <summary>Id de usuario si el remitente estaba autenticado.</summary>
    [MaxLength(450)]
    public string? FromUserId { get; set; }

    public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

    public DateTimeOffset CreatedAt { get; set; }
}
