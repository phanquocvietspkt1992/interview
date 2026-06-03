using banking_monolithic.Models;
using banking_monolithic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace banking_monolithic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(AccountService accountService) : ControllerBase
    {
        public record OpenAccountRequest(Guid CustomerId, AccountType Type, decimal InitialDeposit);

        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> Open(OpenAccountRequest req)
        {
            try
            {
                var account = await accountService.OpenAccount(req.CustomerId, req.Type, req.InitialDeposit);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var account = await accountService.GetAccountAsync(id);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

        }
        //[HttpGet("{id:guid}/transactions")]
        //public async Task<IActionResult> GetTransactions(Guid id)
        //       => Ok(await accountService.Get(id));

        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetByCustomer(Guid customerId)
            => Ok(await accountService.GetCustomerAccountsAsync(customerId));
    }
}
