using LoanService.Application.Commands.ApplyForLoan;
using LoanService.Application.Commands.ApproveLoan;
using LoanService.Application.Commands.MakePayment;
using LoanService.Application.Commands.RejectLoan;
using LoanService.Application.DTOs;
using LoanService.Application.Queries.GetLoan;
using LoanService.Application.Queries.GetLoansByCustomer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoanService.API.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var loan = await mediator.Send(new GetLoanQuery(id), ct);
        return Ok(loan);
    }

    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken ct)
    {
        var loans = await mediator.Send(new GetLoansByCustomerQuery(customerId), ct);
        return Ok(loans);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Apply([FromBody] ApplyForLoanRequest req, CancellationToken ct)
    {
        var loan = await mediator.Send(
            new ApplyForLoanCommand(req.CustomerId, req.AccountId, req.Amount, req.InterestRate, req.TermMonths), ct);

        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var loan = await mediator.Send(new ApproveLoanCommand(id), ct);
        return Ok(loan);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLoanRequest req, CancellationToken ct)
    {
        var loan = await mediator.Send(new RejectLoanCommand(id, req.Reason), ct);
        return Ok(loan);
    }

    [HttpPost("{id:guid}/payment")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MakePayment(Guid id, [FromBody] MakePaymentRequest req, CancellationToken ct)
    {
        var loan = await mediator.Send(new MakePaymentCommand(id, req.Amount), ct);
        return Ok(loan);
    }
}
