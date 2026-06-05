using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public record NotificationDto(
    Guid Id,
    Guid CustomerId,
    NotificationChannel Channel,
    string Subject,
    string Body,
    string Recipient,
    NotificationStatus Status,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? SentAt
);

public record SendNotificationRequest(
    Guid CustomerId,
    NotificationChannel Channel,
    string Subject,
    string Body,
    string Recipient
);
