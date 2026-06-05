using AccountService.Application.DTOs;
using AccountService.Domain.Exceptions;
using AccountService.Domain.Repositories;
using MediatR;

namespace AccountService.Application.Commands.Deposit;

public record DepositCommand(Guid AccountId, decimal Amount) : IRequest<AccountDto>;

public class DepositCommandHandler(
    IAccountRepository repository
) : IRequestHandler<DepositCommand, AccountDto>
{
    public async Task<AccountDto> Handle(DepositCommand cmd, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(cmd.AccountId, ct)
            ?? throw new AccountNotFoundException(cmd.AccountId);

        // Business rule enforced in the aggregate — handler stays thin
        account.Deposit(cmd.Amount);

        return account.ToDto();
    }
}
