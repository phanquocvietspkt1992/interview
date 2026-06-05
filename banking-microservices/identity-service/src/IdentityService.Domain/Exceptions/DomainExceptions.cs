namespace IdentityService.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class CustomerNotFoundException(Guid id)
    : DomainException($"Customer {id} was not found");

public class DuplicateEmailException(string email)
    : DomainException($"A customer with email '{email}' already exists");
