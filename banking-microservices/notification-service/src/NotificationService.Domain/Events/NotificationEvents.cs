namespace NotificationService.Domain.Events;

public record NotificationSentEvent(Guid NotificationId, Guid CustomerId, string Channel) : DomainEvent;
public record NotificationFailedEvent(Guid NotificationId, string Reason) : DomainEvent;
