using MediatR;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.Queries.GetNotificationsByCustomer;

public record GetNotificationsByCustomerQuery(Guid CustomerId) : IRequest<List<NotificationDto>>;

public class GetNotificationsByCustomerQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetNotificationsByCustomerQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetNotificationsByCustomerQuery query, CancellationToken ct)
    {
        var notifications = await repository.GetByCustomerIdAsync(query.CustomerId, ct);
        return notifications.Select(n => n.ToDto()).ToList();
    }
}
