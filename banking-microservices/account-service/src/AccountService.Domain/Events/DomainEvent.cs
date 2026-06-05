namespace AccountService.Domain.Events;

/// <summary>
/// Base class for all domain events.
/// Domain events represent something that HAPPENED in the domain — past tense, immutable.
/// They are raised inside the aggregate and dispatched after the transaction commits.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
