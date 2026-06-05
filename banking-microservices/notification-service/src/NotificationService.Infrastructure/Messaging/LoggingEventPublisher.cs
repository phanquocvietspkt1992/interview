using Microsoft.Extensions.Logging;
using NotificationService.Application.Common;
using NotificationService.Domain.Events;

namespace NotificationService.Infrastructure.Messaging;

public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[Domain Event] {EventType} at {OccurredAt} — {Event}",
            domainEvent.GetType().Name,
            domainEvent.OccurredAt,
            domainEvent);
        return Task.CompletedTask;
    }
}
