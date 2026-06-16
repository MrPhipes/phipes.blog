using Phipes.Blog.Domain;

namespace Phipes.Blog.Abstractions;

/// <summary>
/// Notifica al dueño del sitio cuando llega un mensaje de contacto. El envío real
/// (correo vía Microsoft Graph, etc.) es responsabilidad del host: el paquete solo
/// persiste el mensaje y dispara este <i>seam</i>. La implementación por defecto no hace nada.
/// </summary>
public interface IContactNotifier
{
    Task NotifyAsync(ContactMessage message, CancellationToken cancellationToken = default);
}

/// <summary>No-op: persiste el mensaje pero no envía nada. El host enchufa su propio notificador.</summary>
public sealed class NullContactNotifier : IContactNotifier
{
    public Task NotifyAsync(ContactMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
