using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public string Reference { get; private set; } = default!;

    public Guid AccountId { get; private set; }
    public string ExternalAccountNumber { get; private set; } = default!;
    public string BeneficiaryName { get; private set; } = default!;

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public PaymentNetwork Network { get; private set; }
    public PaymentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }
    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Payment() { }

    public static Payment Initiate(
        Guid accountId,
        string externalAccountNumber,
        string beneficiaryName,
        decimal amount,
        string currency,
        PaymentNetwork network,
        string? description = null)
    {
        if (amount <= 0) throw new InvalidPaymentException("Amount must be positive");
        if (string.IsNullOrWhiteSpace(externalAccountNumber))
            throw new InvalidPaymentException("External account number is required");
        if (string.IsNullOrWhiteSpace(beneficiaryName))
            throw new InvalidPaymentException("Beneficiary name is required");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Reference = GenerateReference(network),
            AccountId = accountId,
            ExternalAccountNumber = externalAccountNumber.Trim(),
            BeneficiaryName = beneficiaryName.Trim(),
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Network = network,
            Status = PaymentStatus.Pending,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        payment.Raise(new PaymentInitiatedEvent(payment.Id, accountId, amount, network.ToString()));
        return payment;
    }

    public void Process()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidPaymentException("Only pending payments can be processed");

        Status = PaymentStatus.Processing;
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidPaymentException("Only processing payments can be completed");

        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;

        Raise(new PaymentCompletedEvent(Id, Amount));
    }

    public void Fail(string reason)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Reversed)
            throw new InvalidPaymentException("Cannot fail a completed or reversed payment");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;

        Raise(new PaymentFailedEvent(Id, reason));
    }

    private void Raise(DomainEvent e) => _domainEvents.Add(e);

    private static string GenerateReference(PaymentNetwork network)
        => $"{network.ToString().ToUpper()[..3]}{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
}
