using MediatR;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Repositories;

namespace TransactionService.Application.Commands.RecordDeposit;

public record RecordDepositCommand(Guid AccountId, decimal Amount, string? Description) : IRequest<TransactionDto>;

public class RecordDepositCommandHandler(
    ITransactionRepository repository
) : IRequestHandler<RecordDepositCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(RecordDepositCommand cmd, CancellationToken ct)
    {
        var transaction = Transaction.Initiate(
            cmd.AccountId,
            null,
            cmd.Amount,
            TransactionType.Deposit,
            cmd.Description);

        transaction.Complete();

        await repository.AddAsync(transaction, ct);
        return transaction.ToDto();
    }
}
