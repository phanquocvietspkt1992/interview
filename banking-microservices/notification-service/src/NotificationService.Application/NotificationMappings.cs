using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application;

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification n) => new(
        n.Id,
        n.CustomerId,
        n.Channel,
        n.Subject,
        n.Body,
        n.Recipient,
        n.Status,
        n.FailureReason,
        n.CreatedAt,
        n.SentAt
    );
}
