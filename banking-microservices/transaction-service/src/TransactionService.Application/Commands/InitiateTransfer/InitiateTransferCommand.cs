using MediatR;
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
    ITransactionRepository repository
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

        // In a real system, this would call account-service to debit/credit accounts
        // and use a Saga to coordinate. For now we complete immediately.
        transaction.Complete();

        await repository.AddAsync(transaction, ct);
        return transaction.ToDto();
    }
}
