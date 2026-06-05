using AccountService.Application.DTOs;
using AccountService.Domain.Exceptions;
using AccountService.Domain.Repositories;
using MediatR;

namespace AccountService.Application.Queries.GetAccount;

// ── Query ──────────────────────────────────────────────────────────────────
// A Query READS state and returns data — it never changes anything.
// CQRS separates reads (Query) from writes (Command).
// Benefit: you can optimise queries separately (e.g., raw SQL, read replicas).
public record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;

public class GetAccountQueryHandler(
    IAccountRepository repository
) : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery query, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(query.AccountId, ct)
            ?? throw new AccountNotFoundException(query.AccountId);

        return account.ToDto();
    }
}
