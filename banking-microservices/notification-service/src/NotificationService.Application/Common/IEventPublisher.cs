using NotificationService.Domain.Events;

namespace NotificationService.Application.Common;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
