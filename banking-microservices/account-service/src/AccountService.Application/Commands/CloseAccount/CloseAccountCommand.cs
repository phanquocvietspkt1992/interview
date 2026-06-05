using AccountService.Application.DTOs;
using AccountService.Domain.Exceptions;
using AccountService.Domain.Repositories;
using MediatR;

namespace AccountService.Application.Commands.CloseAccount;

public record CloseAccountCommand(Guid AccountId) : IRequest<AccountDto>;

public class CloseAccountCommandHandler(
    IAccountRepository repository
) : IRequestHandler<CloseAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CloseAccountCommand cmd, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(cmd.AccountId, ct)
            ?? throw new AccountNotFoundException(cmd.AccountId);

        account.Close();

        return account.ToDto();
    }
}
