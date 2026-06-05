using AccountService.Application.Commands.CloseAccount;
using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.Commands.Deposit;
using AccountService.Application.Commands.Withdraw;
using AccountService.Application.DTOs;
using AccountService.Application.Queries.GetAccount;
using AccountService.Application.Queries.GetAccountsByCustomer;
using AccountService.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    // ── Queries (reads) ────────────────────────────────────────────────────

    /// <summary>GET /api/accounts/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var account = await mediator.Send(new GetAccountQuery(id), ct);
        return Ok(account);
    }

    /// <summary>GET /api/accounts?customerId={customerId}</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer([FromQuery] Guid customerId, CancellationToken ct)
    {
        var accounts = await mediator.Send(new GetAccountsByCustomerQuery(customerId), ct);
        return Ok(accounts);
    }

    // ── Commands (writes) ──────────────────────────────────────────────────

    /// <summary>POST /api/accounts</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest req, CancellationToken ct)
    {
        var account = await mediator.Send(
            new CreateAccountCommand(req.CustomerId, req.Type, req.InitialDeposit), ct);

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    /// <summary>POST /api/accounts/{id}/deposit</summary>
    [HttpPost("{id:guid}/deposit")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest req, CancellationToken ct)
    {
        var account = await mediator.Send(new DepositCommand(id, req.Amount), ct);
        return Ok(account);
    }

    /// <summary>POST /api/accounts/{id}/withdraw</summary>
    [HttpPost("{id:guid}/withdraw")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] WithdrawRequest req, CancellationToken ct)
    {
        var account = await mediator.Send(new WithdrawCommand(id, req.Amount), ct);
        return Ok(account);
    }

    /// <summary>POST /api/accounts/{id}/close</summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        var account = await mediator.Send(new CloseAccountCommand(id), ct);
        return Ok(account);
    }
}
