namespace LoanService.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class LoanNotFoundException(Guid id)
    : DomainException($"Loan {id} was not found");

public class InvalidLoanOperationException(string reason)
    : DomainException($"Invalid loan operation: {reason}");
