using BankingApp.Models;
using BankingApp.Repositories;
using BankingApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(AccountService accountService, TransactionRepository txRepo) : ControllerBase
{
    public record OpenAccountRequest(Guid CustomerId, AccountType Type, decimal InitialDeposit);

    [HttpPost]
    public async Task<IActionResult> Open(OpenAccountRequest req)
    {
        try
        {
            var account = await accountService.OpenAccountAsync(req.CustomerId, req.Type, req.InitialDeposit);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var account = await accountService.GetAccountAsync(id);
            return Ok(account);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id)
        => Ok(await txRepo.GetByAccountAsync(id));

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
        => Ok(await accountService.GetCustomerAccountsAsync(customerId));
}
