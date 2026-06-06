namespace BuildingBlocks.Messaging.IntegrationEvents;

// ── Saga initiator (published by TransactionService when a transfer is requested) ──────────
public record TransferSagaStarted(
    Guid CorrelationId,
    Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount);

// ── Saga Commands (sent TO services by the saga orchestrator) ────────────────────────────
public record DebitAccountCommand(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    decimal Amount);

public record CreditAccountCommand(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    decimal Amount);

public record ReverseDebitCommand(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    decimal Amount);

public record ProcessPaymentCommand(
    Guid CorrelationId,
    Guid TransactionId,
    Guid FromAccountId,
    Guid? ToAccountId,
    decimal Amount);

// ── Events published BY AccountService (consumed by saga) ───────────────────────────────
public record AccountDebitedEvent(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    decimal Amount);

public record AccountDebitFailedEvent(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    string Reason);

public record AccountCreditedEvent(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    decimal Amount);

public record AccountDebitReversedEvent(
    Guid CorrelationId,
    Guid TransactionId,
    Guid AccountId,
    decimal Amount);

// ── Events published BY PaymentService (consumed by saga) ───────────────────────────────
public record PaymentProcessedEvent(
    Guid CorrelationId,
    Guid TransactionId,
    Guid PaymentId);

public record PaymentFailedEvent(
    Guid CorrelationId,
    Guid TransactionId,
    string Reason);

// ── Kafka audit events (published by TransactionService to Kafka) ────────────────────────
public record TransactionCompletedAuditEvent(
    Guid TransactionId,
    Guid FromAccountId,
    Guid? ToAccountId,
    decimal Amount,
    DateTime CompletedAt);

public record TransactionFailedAuditEvent(
    Guid TransactionId,
    string Reason,
    DateTime FailedAt);
