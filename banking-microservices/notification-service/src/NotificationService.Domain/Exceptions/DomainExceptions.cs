namespace NotificationService.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class NotificationNotFoundException(Guid id)
    : DomainException($"Notification {id} was not found");
