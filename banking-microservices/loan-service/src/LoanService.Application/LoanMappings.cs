using LoanService.Application.DTOs;
using LoanService.Domain.Entities;

namespace LoanService.Application;

public static class LoanMappings
{
    public static LoanDto ToDto(this Loan l) => new(
        l.Id,
        l.CustomerId,
        l.AccountId,
        l.PrincipalAmount,
        l.InterestRate,
        l.TermMonths,
        l.OutstandingBalance,
        l.MonthlyPayment,
        l.Status,
        l.RejectionReason,
        l.AppliedAt,
        l.ApprovedAt,
        l.PaidOffAt
    );
}
