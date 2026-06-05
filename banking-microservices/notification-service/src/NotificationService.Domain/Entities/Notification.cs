using NotificationService.Domain.Enums;
using NotificationService.Domain.Events;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public string Recipient { get; private set; } = default!;
    public NotificationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Notification() { }

    public static Notification Create(
        Guid customerId,
        NotificationChannel channel,
        string subject,
        string body,
        string recipient)
    {
        if (string.IsNullOrWhiteSpace(subject)) throw new DomainException("Notification subject is required");
        if (string.IsNullOrWhiteSpace(body)) throw new DomainException("Notification body is required");
        if (string.IsNullOrWhiteSpace(recipient)) throw new DomainException("Recipient is required");

        return new Notification
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Channel = channel,
            Subject = subject,
            Body = body,
            Recipient = recipient,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkSent()
    {
        if (Status != NotificationStatus.Pending)
            throw new DomainException("Only pending notifications can be marked as sent");

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        _domainEvents.Add(new NotificationSentEvent(Id, CustomerId, Channel.ToString()));
    }

    public void MarkFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;

        _domainEvents.Add(new NotificationFailedEvent(Id, reason));
    }
}
