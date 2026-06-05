namespace AccountService.Domain.Exceptions;

/// <summary>
/// Base for all domain rule violations.
/// These are NOT infrastructure errors — they represent broken business rules.
/// Return HTTP 422 Unprocessable Entity, not 500.
/// </summary>
public class DomainException(string message) : Exception(message);

public class InsufficientFundsException(Guid accountId, decimal requested, decimal available)
    : DomainException($"Account {accountId}: requested {requested:C}, available {available:C}");

public class AccountNotFoundException(Guid accountId)
    : DomainException($"Account {accountId} was not found");

public class AccountNotActiveException(Guid accountId, string status)
    : DomainException($"Account {accountId} is {status} — operation not allowed");
