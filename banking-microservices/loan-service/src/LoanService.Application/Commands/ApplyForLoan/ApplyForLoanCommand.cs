using LoanService.Application.DTOs;
using LoanService.Domain.Entities;
using LoanService.Domain.Repositories;
using MediatR;

namespace LoanService.Application.Commands.ApplyForLoan;

public record ApplyForLoanCommand(
    Guid CustomerId,
    Guid AccountId,
    decimal Amount,
    decimal InterestRate,
    int TermMonths
) : IRequest<LoanDto>;

public class ApplyForLoanCommandHandler(
    ILoanRepository repository
) : IRequestHandler<ApplyForLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(ApplyForLoanCommand cmd, CancellationToken ct)
    {
        var loan = Loan.Apply(cmd.CustomerId, cmd.AccountId, cmd.Amount, cmd.InterestRate, cmd.TermMonths);
        await repository.AddAsync(loan, ct);
        return loan.ToDto();
    }
}
