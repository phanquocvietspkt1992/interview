using AccountService.Domain.Exceptions;
using AccountService.Domain.Repositories;
using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Credits the destination account after payment is processed.
/// Published by the saga orchestrator once PaymentProcessedEvent is received.
/// </summary>
public class CreditAccountConsumer(
    IAccountRepository repository,
    IPublishEndpoint publishEndpoint,
    ILogger<CreditAccountConsumer> logger) : IConsumer<CreditAccountCommand>
{
    public async Task Consume(ConsumeContext<CreditAccountCommand> context)
    {
        var cmd = context.Message;
        logger.LogInformation("Crediting {Amount} to account {AccountId} for transaction {TransactionId}",
            cmd.Amount, cmd.AccountId, cmd.TransactionId);

        var account = await repository.GetByIdAsync(cmd.AccountId, context.CancellationToken);
        if (account is null)
        {
            logger.LogError("Cannot credit — account {AccountId} not found", cmd.AccountId);
            return;
        }

        account.Deposit(cmd.Amount);
        await repository.UpdateAsync(account, context.CancellationToken);

        await publishEndpoint.Publish(new AccountCreditedEvent(
            cmd.CorrelationId, cmd.TransactionId, cmd.AccountId, cmd.Amount),
            context.CancellationToken);

        logger.LogInformation("Account {AccountId} credited {Amount} successfully", cmd.AccountId, cmd.Amount);
    }
}
