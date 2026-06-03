using banking_monolithic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace banking_monolithic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(CustomerService customerService) : ControllerBase
    {
        public record RegisterRequest(string FullName, string Email, string Phone);
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            try
            {
                var customer = await customerService.GetCustomerAsync(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Registry(RegisterRequest req)
        {
            var customer = await customerService.RegistryAsync(req.FullName, req.Email, req.Phone);
            return Ok(customer);
        }
    }
}
