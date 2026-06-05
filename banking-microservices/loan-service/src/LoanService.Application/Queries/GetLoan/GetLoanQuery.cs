using LoanService.Application.DTOs;
using LoanService.Domain.Exceptions;
using LoanService.Domain.Repositories;
using MediatR;

namespace LoanService.Application.Queries.GetLoan;

public record GetLoanQuery(Guid LoanId) : IRequest<LoanDto>;

public class GetLoanQueryHandler(ILoanRepository repository) : IRequestHandler<GetLoanQuery, LoanDto>
{
    public async Task<LoanDto> Handle(GetLoanQuery query, CancellationToken ct)
    {
        var loan = await repository.GetByIdAsync(query.LoanId, ct)
            ?? throw new LoanNotFoundException(query.LoanId);

        return loan.ToDto();
    }
}
