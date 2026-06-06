using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Repositories;
using TransactionService.Infrastructure.Messaging;

namespace TransactionService.Infrastructure.Sagas;

/// <summary>
/// Listens for saga terminal events to update the Transaction entity and publish Kafka audit events.
/// Separated from the state machine to keep the saga pure.
/// </summary>
public class TransferCompletedConsumer(
    ITransactionRepository transactionRepository,
    IKafkaAuditPublisher kafkaPublisher,
    ILogger<TransferCompletedConsumer> logger) : IConsumer<AccountCreditedEvent>
{
    public async Task Consume(ConsumeContext<AccountCreditedEvent> context)
    {
        var transactionId = context.Message.TransactionId;

        var transaction = await transactionRepository.GetByIdAsync(transactionId, context.CancellationToken);
        if (transaction is null)
        {
            logger.LogWarning("Transaction {Id} not found for completion", transactionId);
            return;
        }

        transaction.Complete();
        await transactionRepository.UpdateAsync(transaction, context.CancellationToken);

        await kafkaPublisher.PublishAsync("transaction-completed", new TransactionCompletedAuditEvent(
            transaction.Id,
            transaction.FromAccountId,
            transaction.ToAccountId,
            transaction.Amount,
            DateTime.UtcNow));

        logger.LogInformation("Transfer {TransactionId} completed successfully", transactionId);
    }
}

public class TransferFailedConsumer(
    ITransactionRepository transactionRepository,
    IKafkaAuditPublisher kafkaPublisher,
    ILogger<TransferFailedConsumer> logger) : IConsumer<AccountDebitFailedEvent>
{
    public async Task Consume(ConsumeContext<AccountDebitFailedEvent> context)
    {
        await MarkFailedAsync(context.Message.TransactionId, context.Message.Reason, context.CancellationToken);
    }

    private async Task MarkFailedAsync(Guid transactionId, string reason, CancellationToken ct)
    {
        var transaction = await transactionRepository.GetByIdAsync(transactionId, ct);
        if (transaction is null)
        {
            logger.LogWarning("Transaction {Id} not found for failure", transactionId);
            return;
        }

        transaction.Fail(reason);
        await transactionRepository.UpdateAsync(transaction, ct);

        await kafkaPublisher.PublishAsync("transaction-failed", new TransactionFailedAuditEvent(
            transaction.Id, reason, DateTime.UtcNow));

        logger.LogWarning("Transfer {TransactionId} failed: {Reason}", transactionId, reason);
    }
}

public class TransferPaymentFailedConsumer(
    ITransactionRepository transactionRepository,
    IKafkaAuditPublisher kafkaPublisher,
    ILogger<TransferPaymentFailedConsumer> logger) : IConsumer<AccountDebitReversedEvent>
{
    public async Task Consume(ConsumeContext<AccountDebitReversedEvent> context)
    {
        var transactionId = context.Message.TransactionId;
        var transaction = await transactionRepository.GetByIdAsync(transactionId, context.CancellationToken);
        if (transaction is null) return;

        transaction.Fail("Payment failed — debit reversed");
        await transactionRepository.UpdateAsync(transaction, context.CancellationToken);

        await kafkaPublisher.PublishAsync("transaction-failed", new TransactionFailedAuditEvent(
            transaction.Id, "Payment failed — debit reversed", DateTime.UtcNow));
    }
}
