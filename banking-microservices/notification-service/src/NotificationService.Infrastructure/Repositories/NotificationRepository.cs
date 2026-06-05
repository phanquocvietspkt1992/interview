using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Common;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository(NotificationDbContext db, IEventPublisher eventPublisher) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public Task<List<Notification>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct)
        => db.Notifications
             .Where(n => n.CustomerId == customerId)
             .OrderByDescending(n => n.CreatedAt)
             .ToListAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct)
    {
        await db.Notifications.AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(notification, ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(notification, ct);
    }

    private async Task DispatchEventsAsync(Notification notification, CancellationToken ct)
    {
        foreach (var e in notification.DomainEvents)
            await eventPublisher.PublishAsync(e, ct);
        notification.ClearDomainEvents();
    }
}
