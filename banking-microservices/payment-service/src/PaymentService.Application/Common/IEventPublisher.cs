using PaymentService.Domain.Events;

namespace PaymentService.Application.Common;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
