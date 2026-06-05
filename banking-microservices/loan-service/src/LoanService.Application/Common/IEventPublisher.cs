using LoanService.Domain.Events;

namespace LoanService.Application.Common;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
