using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Repositories;

namespace PaymentService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Receives ProcessPaymentCommand from the Transfer Saga orchestrator.
/// Processes the payment and publishes PaymentProcessedEvent or PaymentFailedEvent.
///
/// Simulates a ~95% success rate — in production this calls an external payment network (SWIFT, Visa).
/// </summary>
public class ProcessPaymentConsumer(
    IPaymentRepository paymentRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<ProcessPaymentConsumer> logger) : IConsumer<ProcessPaymentCommand>
{
    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        var cmd = context.Message;
        logger.LogInformation("Processing payment for transaction {TransactionId}, amount {Amount}",
            cmd.TransactionId, cmd.Amount);

        var payment = Payment.Initiate(
            cmd.FromAccountId,
            cmd.ToAccountId?.ToString() ?? "INTERNAL",
            "Internal Transfer",
            cmd.Amount,
            "USD",
            PaymentNetwork.Internal,
            $"Transfer {cmd.TransactionId}");

        payment.Process();

        // Simulate payment processing (95% success)
        var success = Random.Shared.NextDouble() > 0.05;

        if (success)
        {
            payment.Complete();
            await paymentRepository.AddAsync(payment, context.CancellationToken);

            await publishEndpoint.Publish(new PaymentProcessedEvent(
                cmd.CorrelationId, cmd.TransactionId, payment.Id),
                context.CancellationToken);

            logger.LogInformation("Payment {PaymentId} processed successfully", payment.Id);
        }
        else
        {
            const string reason = "Payment declined by payment network";
            payment.Fail(reason);
            await paymentRepository.AddAsync(payment, context.CancellationToken);

            await publishEndpoint.Publish(new PaymentFailedEvent(
                cmd.CorrelationId, cmd.TransactionId, reason),
                context.CancellationToken);

            logger.LogWarning("Payment failed for transaction {TransactionId}", cmd.TransactionId);
        }
    }
}
