using LoanService.Application.DTOs;
using LoanService.Domain.Repositories;
using MediatR;

namespace LoanService.Application.Queries.GetLoansByCustomer;

public record GetLoansByCustomerQuery(Guid CustomerId) : IRequest<List<LoanDto>>;

public class GetLoansByCustomerQueryHandler(ILoanRepository repository)
    : IRequestHandler<GetLoansByCustomerQuery, List<LoanDto>>
{
    public async Task<List<LoanDto>> Handle(GetLoansByCustomerQuery query, CancellationToken ct)
    {
        var loans = await repository.GetByCustomerIdAsync(query.CustomerId, ct);
        return loans.Select(l => l.ToDto()).ToList();
    }
}
