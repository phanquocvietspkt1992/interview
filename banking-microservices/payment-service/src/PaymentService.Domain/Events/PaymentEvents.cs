namespace PaymentService.Domain.Events;

public record PaymentInitiatedEvent(Guid PaymentId, Guid AccountId, decimal Amount, string Network) : DomainEvent;
public record PaymentCompletedEvent(Guid PaymentId, decimal Amount) : DomainEvent;
public record PaymentFailedEvent(Guid PaymentId, string Reason) : DomainEvent;
