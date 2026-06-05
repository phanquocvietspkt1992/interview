using AccountService.Domain.Events;

namespace AccountService.Application.Common;

/// <summary>
/// After SaveChanges(), the infrastructure calls this to publish domain events
/// to the message bus (RabbitMQ, Azure Service Bus, etc.).
/// The Application layer depends on this interface, not a concrete MassTransit/RabbitMQ class.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
