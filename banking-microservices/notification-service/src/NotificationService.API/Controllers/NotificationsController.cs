using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Commands.SendNotification;
using NotificationService.Application.DTOs;
using NotificationService.Application.Queries.GetNotificationsByCustomer;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken ct)
    {
        var notifications = await mediator.Send(new GetNotificationsByCustomerQuery(customerId), ct);
        return Ok(notifications);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest req, CancellationToken ct)
    {
        var notification = await mediator.Send(
            new SendNotificationCommand(req.CustomerId, req.Channel, req.Subject, req.Body, req.Recipient), ct);

        return CreatedAtAction(null, new { id = notification.Id }, notification);
    }
}
