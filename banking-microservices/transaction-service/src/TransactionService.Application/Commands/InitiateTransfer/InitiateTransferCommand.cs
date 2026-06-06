using System.Text.Json;
using BuildingBlocks.Messaging.IntegrationEvents;
using MediatR;
using TransactionService.Application.Common;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Repositories;

namespace TransactionService.Application.Commands.InitiateTransfer;

public record InitiateTransferCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description
) : IRequest<TransactionDto>;

public class InitiateTransferCommandHandler(
    ITransactionRepository transactionRepository,
    IOutboxRepository outboxRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<InitiateTransferCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(InitiateTransferCommand cmd, CancellationToken ct)
    {
        var transaction = Transaction.Initiate(
            cmd.FromAccountId,
            cmd.ToAccountId,
            cmd.Amount,
            TransactionType.Transfer,
            cmd.Description);

        // ── Outbox Pattern ──────────────────────────────────────────────────
        // PROBLEM: If we save the Transaction to DB and then publish to RabbitMQ,
        // and the RabbitMQ publish fails (network blip, broker down), we have a saga that
        // never starts — money is neither moved nor the transaction marked failed.
        //
        // SOLUTION: Write Transaction + OutboxMessage atomically in ONE SaveChanges().
        // The OutboxProcessor (BackgroundService) reliably publishes the message later.
        // Even if the service crashes before publishing, the next startup will re-publish.

        var correlationId = Guid.NewGuid();
        var sagaEvent = new TransferSagaStarted(
            correlationId,
            transaction.Id,
            transaction.FromAccountId,
            transaction.ToAccountId ?? Guid.Empty,
            transaction.Amount);

        await transactionRepository.AddAsync(transaction, ct);
        await outboxRepository.AddAsync(
            typeof(TransferSagaStarted).AssemblyQualifiedName!,
            JsonSerializer.Serialize(sagaEvent),
            ct);

        await unitOfWork.CommitAsync(ct); // one atomic DB transaction

        return transaction.ToDto();
    }
}
