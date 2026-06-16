using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Recepción y gestión de mensajes del formulario de contacto.</summary>
public interface IContactService
{
    Task<int> SubmitAsync(ContactMessage message, CancellationToken ct = default);
    Task<PagedResult<ContactMessage>> ListAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task SetStatusAsync(int id, ContactMessageStatus status, CancellationToken ct = default);
}

/// <summary>Implementación EF Core: persiste el mensaje y dispara el notificador del host.</summary>
public sealed class EfContactService(
    PhipesBlogDbContext db,
    IContactNotifier notifier,
    IBlogUserContext user) : IContactService
{
    public async Task<int> SubmitAsync(ContactMessage message, CancellationToken ct = default)
    {
        message.CreatedAt = DateTimeOffset.UtcNow;
        message.Status = ContactMessageStatus.New;
        if (user.IsAuthenticated) message.FromUserId = user.UserId;

        db.ContactMessages.Add(message);
        await db.SaveChangesAsync(ct);

        await notifier.NotifyAsync(message, ct);
        return message.Id;
    }

    public async Task<PagedResult<ContactMessage>> ListAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        var total = await db.ContactMessages.CountAsync(ct);
        var items = await db.ContactMessages.AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ContactMessage>(items, page, pageSize, total);
    }

    public async Task SetStatusAsync(int id, ContactMessageStatus status, CancellationToken ct = default)
    {
        var msg = await db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg is null) return;
        msg.Status = status;
        await db.SaveChangesAsync(ct);
    }
}
