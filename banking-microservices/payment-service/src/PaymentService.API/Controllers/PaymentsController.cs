using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands.InitiatePayment;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries.GetPayment;
using PaymentService.Application.Queries.GetPaymentsByAccount;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var payment = await mediator.Send(new GetPaymentQuery(id), ct);
        return Ok(payment);
    }

    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAccount(Guid accountId, CancellationToken ct)
    {
        var payments = await mediator.Send(new GetPaymentsByAccountQuery(accountId), ct);
        return Ok(payments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest req, CancellationToken ct)
    {
        var payment = await mediator.Send(
            new InitiatePaymentCommand(
                req.AccountId,
                req.ExternalAccountNumber,
                req.BeneficiaryName,
                req.Amount,
                req.Currency,
                req.Network,
                req.Description), ct);

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }
}
