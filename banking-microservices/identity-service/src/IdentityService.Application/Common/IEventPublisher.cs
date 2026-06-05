using IdentityService.Domain.Events;

namespace IdentityService.Application.Common;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
