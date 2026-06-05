using LoanService.Application.DTOs;
using LoanService.Domain.Exceptions;
using LoanService.Domain.Repositories;
using MediatR;

namespace LoanService.Application.Commands.MakePayment;

public record MakePaymentCommand(Guid LoanId, decimal Amount) : IRequest<LoanDto>;

public class MakePaymentCommandHandler(
    ILoanRepository repository
) : IRequestHandler<MakePaymentCommand, LoanDto>
{
    public async Task<LoanDto> Handle(MakePaymentCommand cmd, CancellationToken ct)
    {
        var loan = await repository.GetByIdAsync(cmd.LoanId, ct)
            ?? throw new LoanNotFoundException(cmd.LoanId);

        loan.MakePayment(cmd.Amount);
        await repository.UpdateAsync(loan, ct);

        return loan.ToDto();
    }
}
