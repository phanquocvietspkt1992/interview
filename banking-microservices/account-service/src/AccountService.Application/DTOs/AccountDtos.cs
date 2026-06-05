using AccountService.Domain.Enums;

namespace AccountService.Application.DTOs;

/// <summary>
/// Data Transfer Object — what we return to the API caller.
/// Never expose the Domain entity directly over HTTP; the DTO is a stable contract.
/// </summary>
public record AccountDto(
    Guid Id,
    string AccountNumber,
    Guid CustomerId,
    decimal Balance,
    string Type,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAccountRequest(
    Guid CustomerId,
    AccountType Type,
    decimal InitialDeposit
);

public record DepositRequest(decimal Amount);
public record WithdrawRequest(decimal Amount);
public record FreezeRequest(string Reason);
