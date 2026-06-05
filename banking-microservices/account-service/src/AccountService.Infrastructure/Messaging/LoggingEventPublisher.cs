using AccountService.Application.Common;
using AccountService.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.Messaging;

/// <summary>
/// Logs domain events to the console.
/// 
/// In production, swap this for a real implementation:
///   - MassTransit + RabbitMQ  → publish to an exchange
///   - Azure Service Bus       → send to a topic
///   - Kafka                   → produce to a topic
///
/// Because the API only depends on IEventPublisher (defined in Application),
/// swapping implementations requires zero changes outside this class.
/// That's the power of the Dependency Inversion Principle.
/// </summary>
public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[DomainEvent] {EventType} | Id={EventId} | OccurredAt={OccurredAt} | Payload={@Payload}",
            domainEvent.GetType().Name,
            domainEvent.EventId,
            domainEvent.OccurredAt,
            domainEvent);

        // TODO: replace with:
        // await _bus.Publish(domainEvent, ct);   // MassTransit
        // await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(...), ct);  // Azure

        return Task.CompletedTask;
    }
}
