namespace TransactionService.Domain.Events;

public record TransactionInitiatedEvent(Guid TransactionId, Guid FromAccountId, Guid? ToAccountId, decimal Amount) : DomainEvent;
public record TransactionCompletedEvent(Guid TransactionId, decimal Amount) : DomainEvent;
public record TransactionFailedEvent(Guid TransactionId, string Reason) : DomainEvent;
