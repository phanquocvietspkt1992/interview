using AccountService.Application.DTOs;
using AccountService.Domain.Repositories;
using MediatR;

namespace AccountService.Application.Queries.GetAccountsByCustomer;

public record GetAccountsByCustomerQuery(Guid CustomerId) : IRequest<List<AccountDto>>;

public class GetAccountsByCustomerQueryHandler(
    IAccountRepository repository
) : IRequestHandler<GetAccountsByCustomerQuery, List<AccountDto>>
{
    public async Task<List<AccountDto>> Handle(GetAccountsByCustomerQuery query, CancellationToken ct)
    {
        var accounts = await repository.GetByCustomerIdAsync(query.CustomerId, ct);
        return accounts.Select(a => a.ToDto()).ToList();
    }
}
