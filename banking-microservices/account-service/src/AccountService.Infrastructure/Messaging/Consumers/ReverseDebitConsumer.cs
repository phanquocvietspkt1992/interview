using AccountService.Domain.Repositories;
using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Compensating action: reverses a debit when downstream payment fails.
/// Part of the Saga compensation flow.
/// </summary>
public class ReverseDebitConsumer(
    IAccountRepository repository,
    IPublishEndpoint publishEndpoint,
    ILogger<ReverseDebitConsumer> logger) : IConsumer<ReverseDebitCommand>
{
    public async Task Consume(ConsumeContext<ReverseDebitCommand> context)
    {
        var cmd = context.Message;
        logger.LogWarning("Reversing debit of {Amount} on account {AccountId} (saga compensation)",
            cmd.Amount, cmd.AccountId);

        var account = await repository.GetByIdAsync(cmd.AccountId, context.CancellationToken);
        if (account is null)
        {
            logger.LogError("Cannot reverse debit — account {AccountId} not found", cmd.AccountId);
            return;
        }

        account.Deposit(cmd.Amount); // re-credit the amount that was debited
        await repository.UpdateAsync(account, context.CancellationToken);

        await publishEndpoint.Publish(new AccountDebitReversedEvent(
            cmd.CorrelationId, cmd.TransactionId, cmd.AccountId, cmd.Amount),
            context.CancellationToken);

        logger.LogInformation("Debit reversal complete for account {AccountId}", cmd.AccountId);
    }
}
