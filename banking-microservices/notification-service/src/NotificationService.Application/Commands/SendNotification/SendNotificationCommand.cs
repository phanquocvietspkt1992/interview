using MediatR;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.Commands.SendNotification;

public record SendNotificationCommand(
    Guid CustomerId,
    NotificationChannel Channel,
    string Subject,
    string Body,
    string Recipient
) : IRequest<NotificationDto>;

public class SendNotificationCommandHandler(
    INotificationRepository repository
) : IRequestHandler<SendNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(SendNotificationCommand cmd, CancellationToken ct)
    {
        var notification = Notification.Create(cmd.CustomerId, cmd.Channel, cmd.Subject, cmd.Body, cmd.Recipient);

        // In production this would dispatch to email/SMS/push providers.
        // Here we mark sent immediately (simulated delivery).
        notification.MarkSent();

        await repository.AddAsync(notification, ct);
        return notification.ToDto();
    }
}
