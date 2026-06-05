namespace TransactionService.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class TransactionNotFoundException(Guid id)
    : DomainException($"Transaction {id} was not found");

public class InvalidTransactionException(string reason)
    : DomainException($"Invalid transaction: {reason}");
