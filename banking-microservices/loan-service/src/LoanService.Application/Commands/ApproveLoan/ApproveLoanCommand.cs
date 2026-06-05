using LoanService.Application.DTOs;
using LoanService.Domain.Exceptions;
using LoanService.Domain.Repositories;
using MediatR;

namespace LoanService.Application.Commands.ApproveLoan;

public record ApproveLoanCommand(Guid LoanId) : IRequest<LoanDto>;

public class ApproveLoanCommandHandler(
    ILoanRepository repository
) : IRequestHandler<ApproveLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(ApproveLoanCommand cmd, CancellationToken ct)
    {
        var loan = await repository.GetByIdAsync(cmd.LoanId, ct)
            ?? throw new LoanNotFoundException(cmd.LoanId);

        loan.Approve();
        loan.Activate();
        await repository.UpdateAsync(loan, ct);

        return loan.ToDto();
    }
}
