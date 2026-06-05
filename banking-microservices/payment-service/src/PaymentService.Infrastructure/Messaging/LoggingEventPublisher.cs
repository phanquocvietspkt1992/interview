using Microsoft.Extensions.Logging;
using PaymentService.Application.Common;
using PaymentService.Domain.Events;

namespace PaymentService.Infrastructure.Messaging;

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
