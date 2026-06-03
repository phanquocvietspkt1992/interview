using banking_monolithic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace banking_monolithic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TransferController(TransferService transferService) : ControllerBase
    {
        public record TransferRequest(string FromAccount, string ToAccount, decimal Amount);

        [HttpPost]
        public async Task<IActionResult> Transfer(TransferRequest req)
        {
            try
            {
                await transferService.TransferAsync(req.FromAccount, req.ToAccount, req.Amount);
                return Ok(new { message = $"Transferred ${req.Amount} from {req.FromAccount} to {req.ToAccount}" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}
