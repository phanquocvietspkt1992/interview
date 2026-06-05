using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Commands.InitiateTransfer;
using TransactionService.Application.Commands.RecordDeposit;
using TransactionService.Application.DTOs;
using TransactionService.Application.Queries.GetTransaction;
using TransactionService.Application.Queries.GetTransactionHistory;

namespace TransactionService.API.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tx = await mediator.Send(new GetTransactionQuery(id), ct);
        return Ok(tx);
    }

    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTransactionHistoryQuery(accountId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Transfer([FromBody] InitiateTransferRequest req, CancellationToken ct)
    {
        var tx = await mediator.Send(
            new InitiateTransferCommand(req.FromAccountId, req.ToAccountId, req.Amount, req.Description), ct);

        return CreatedAtAction(nameof(GetById), new { id = tx.Id }, tx);
    }

    [HttpPost("deposit")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Deposit([FromBody] RecordDepositRequest req, CancellationToken ct)
    {
        var tx = await mediator.Send(new RecordDepositCommand(req.AccountId, req.Amount, req.Description), ct);
        return CreatedAtAction(nameof(GetById), new { id = tx.Id }, tx);
    }
}
