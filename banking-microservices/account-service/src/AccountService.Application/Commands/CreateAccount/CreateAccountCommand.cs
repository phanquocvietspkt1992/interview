using AccountService.Application.DTOs;
using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using AccountService.Domain.Repositories;
using MediatR;

namespace AccountService.Application.Commands.CreateAccount;

// ── Command ────────────────────────────────────────────────────────────────
// A Command expresses INTENT: "I want to create an account."
// It is a one-way message — it changes state and returns a result (the new AccountDto).
public record CreateAccountCommand(
    Guid CustomerId,
    AccountType Type,
    decimal InitialDeposit
) : IRequest<AccountDto>;

// ── Handler ────────────────────────────────────────────────────────────────
// One handler per command. MediatR routes the command to this handler automatically.
public class CreateAccountCommandHandler(
    IAccountRepository repository
) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand cmd, CancellationToken ct)
    {
        // 1. Delegate all business logic to the domain aggregate
        var account = Account.Create(cmd.CustomerId, cmd.Type, cmd.InitialDeposit);

        // 2. Persist — Infrastructure will also dispatch domain events after saving
        await repository.AddAsync(account, ct);

        // 3. Map domain entity → DTO (never return the entity itself)
        return account.ToDto();
    }
}
