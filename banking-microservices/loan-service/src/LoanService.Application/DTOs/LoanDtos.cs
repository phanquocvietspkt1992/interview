using LoanService.Domain.Enums;

namespace LoanService.Application.DTOs;

public record LoanDto(
    Guid Id,
    Guid CustomerId,
    Guid AccountId,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    decimal OutstandingBalance,
    decimal MonthlyPayment,
    LoanStatus Status,
    string? RejectionReason,
    DateTime AppliedAt,
    DateTime? ApprovedAt,
    DateTime? PaidOffAt
);

public record ApplyForLoanRequest(
    Guid CustomerId,
    Guid AccountId,
    decimal Amount,
    decimal InterestRate,
    int TermMonths
);

public record ApproveLoanRequest();

public record RejectLoanRequest(string Reason);

public record MakePaymentRequest(decimal Amount);
