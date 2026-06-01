using BankingApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController(CustomerService customerService) : ControllerBase
{
    public record RegisterRequest(string FullName, string Email, string Phone);

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var customer = await customerService.RegisterAsync(req.FullName, req.Email, req.Phone);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var customer = await customerService.GetCustomerAsync(id);
            return Ok(customer);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await customerService.GetAllAsync());
}
