using TransactionService.Domain.Events;

namespace TransactionService.Application.Common;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
