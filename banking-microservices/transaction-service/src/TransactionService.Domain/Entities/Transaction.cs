using TransactionService.Domain.Enums;
using TransactionService.Domain.Events;
using TransactionService.Domain.Exceptions;

namespace TransactionService.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string Reference { get; private set; } = default!;

    public Guid FromAccountId { get; private set; }
    public Guid? ToAccountId { get; private set; }

    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }

    public string? Description { get; private set; }
    public string? FailureReason { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Transaction() { }

    public static Transaction Initiate(
        Guid fromAccountId,
        Guid? toAccountId,
        decimal amount,
        TransactionType type,
        string? description = null)
    {
        if (amount <= 0) throw new InvalidTransactionException("Amount must be positive");
        if (type == TransactionType.Transfer && toAccountId is null)
            throw new InvalidTransactionException("Transfer requires a destination account");
        if (type == TransactionType.Transfer && fromAccountId == toAccountId)
            throw new InvalidTransactionException("Source and destination accounts must differ");

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = GenerateReference(),
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Type = type,
            Status = TransactionStatus.Pending,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        tx.Raise(new TransactionInitiatedEvent(tx.Id, fromAccountId, toAccountId, amount));
        return tx;
    }

    public void Complete()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidTransactionException("Only pending transactions can be completed");

        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        Raise(new TransactionCompletedEvent(Id, Amount));
    }

    public void Fail(string reason)
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidTransactionException("Only pending transactions can be failed");

        Status = TransactionStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;

        Raise(new TransactionFailedEvent(Id, reason));
    }

    private void Raise(DomainEvent e) => _domainEvents.Add(e);

    private static string GenerateReference()
        => $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
}
