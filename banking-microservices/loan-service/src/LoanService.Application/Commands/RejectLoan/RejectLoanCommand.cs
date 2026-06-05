using LoanService.Application.DTOs;
using LoanService.Domain.Exceptions;
using LoanService.Domain.Repositories;
using MediatR;

namespace LoanService.Application.Commands.RejectLoan;

public record RejectLoanCommand(Guid LoanId, string Reason) : IRequest<LoanDto>;

public class RejectLoanCommandHandler(
    ILoanRepository repository
) : IRequestHandler<RejectLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(RejectLoanCommand cmd, CancellationToken ct)
    {
        var loan = await repository.GetByIdAsync(cmd.LoanId, ct)
            ?? throw new LoanNotFoundException(cmd.LoanId);

        loan.Reject(cmd.Reason);
        await repository.UpdateAsync(loan, ct);

        return loan.ToDto();
    }
}
