using AccountService.Application.DTOs;
using AccountService.Domain.Exceptions;
using AccountService.Domain.Repositories;
using MediatR;

namespace AccountService.Application.Commands.Withdraw;

public record WithdrawCommand(Guid AccountId, decimal Amount) : IRequest<AccountDto>;

public class WithdrawCommandHandler(
    IAccountRepository repository
) : IRequestHandler<WithdrawCommand, AccountDto>
{
    public async Task<AccountDto> Handle(WithdrawCommand cmd, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(cmd.AccountId, ct)
            ?? throw new AccountNotFoundException(cmd.AccountId);

        account.Withdraw(cmd.Amount);

        return account.ToDto();
    }
}
