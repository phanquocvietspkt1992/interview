using MassTransit;

namespace TransactionService.Infrastructure.Sagas;

/// <summary>
/// Persistent state for the Transfer Saga.
/// Stored in the TransactionDb via EF Core — MassTransit manages the row lifecycle.
/// CorrelationId ties together all messages belonging to the same transfer.
/// </summary>
public class TransferSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;

    public Guid TransactionId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }

    public Guid? PaymentId { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
