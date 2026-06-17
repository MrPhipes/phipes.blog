using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Recepción y gestión de mensajes del formulario de contacto.</summary>
public interface IContactService
{
    Task<int> SubmitAsync(ContactMessage message, CancellationToken ct = default);
    Task<PagedResult<ContactMessage>> ListAsync(int page = 1, int pageSize = 20, ContactMessageStatus? status = null, CancellationToken ct = default);
    Task<int> CountByStatusAsync(ContactMessageStatus status, CancellationToken ct = default);
    Task SetStatusAsync(int id, ContactMessageStatus status, CancellationToken ct = default);
}

/// <summary>Implementación EF Core: persiste el mensaje y dispara el notificador del host.</summary>
public sealed class EfContactService(
    IPhipesBlogDbContext db,
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

    public async Task<PagedResult<ContactMessage>> ListAsync(int page = 1, int pageSize = 20, ContactMessageStatus? status = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        var query = db.ContactMessages.AsNoTracking();
        if (status is { } s) query = query.Where(m => m.Status == s);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ContactMessage>(items, page, pageSize, total);
    }

    public Task<int> CountByStatusAsync(ContactMessageStatus status, CancellationToken ct = default)
        => db.ContactMessages.CountAsync(m => m.Status == status, ct);

    public async Task SetStatusAsync(int id, ContactMessageStatus status, CancellationToken ct = default)
    {
        var msg = await db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg is null) return;
        msg.Status = status;
        await db.SaveChangesAsync(ct);
    }
}
