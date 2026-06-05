namespace PaymentService.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class PaymentNotFoundException(Guid id)
    : DomainException($"Payment {id} was not found");

public class InvalidPaymentException(string reason)
    : DomainException($"Invalid payment: {reason}");
