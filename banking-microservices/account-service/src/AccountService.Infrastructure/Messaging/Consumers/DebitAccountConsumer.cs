using AccountService.Domain.Exceptions;
using AccountService.Domain.Repositories;
using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Receives DebitAccountCommand from the Transfer Saga.
/// Debits the account and publishes AccountDebitedEvent (success) or AccountDebitFailedEvent (failure).
/// Idempotency: MassTransit re-delivers on consumer crash, but balance debit must not be doubled.
/// In production, store a processed-commands log per (CorrelationId, CommandType).
/// </summary>
public class DebitAccountConsumer(
    IAccountRepository repository,
    IPublishEndpoint publishEndpoint,
    ILogger<DebitAccountConsumer> logger) : IConsumer<DebitAccountCommand>
{
    public async Task Consume(ConsumeContext<DebitAccountCommand> context)
    {
        var cmd = context.Message;
        logger.LogInformation("Debiting {Amount} from account {AccountId} for transaction {TransactionId}",
            cmd.Amount, cmd.AccountId, cmd.TransactionId);

        var account = await repository.GetByIdAsync(cmd.AccountId, context.CancellationToken);

        if (account is null)
        {
            await publishEndpoint.Publish(new AccountDebitFailedEvent(
                cmd.CorrelationId, cmd.TransactionId, cmd.AccountId,
                $"Account {cmd.AccountId} not found"), context.CancellationToken);
            return;
        }

        try
        {
            account.Withdraw(cmd.Amount);
            await repository.UpdateAsync(account, context.CancellationToken);

            await publishEndpoint.Publish(new AccountDebitedEvent(
                cmd.CorrelationId, cmd.TransactionId, cmd.AccountId, cmd.Amount),
                context.CancellationToken);

            logger.LogInformation("Account {AccountId} debited {Amount} successfully", cmd.AccountId, cmd.Amount);
        }
        catch (InsufficientFundsException ex)
        {
            await publishEndpoint.Publish(new AccountDebitFailedEvent(
                cmd.CorrelationId, cmd.TransactionId, cmd.AccountId, ex.Message),
                context.CancellationToken);
        }
        catch (DomainException ex)
        {
            await publishEndpoint.Publish(new AccountDebitFailedEvent(
                cmd.CorrelationId, cmd.TransactionId, cmd.AccountId, ex.Message),
                context.CancellationToken);
        }
    }
}
