using IdentityService.Application.Commands.RegisterCustomer;
using IdentityService.Application.Commands.UpdateCustomer;
using IdentityService.Application.Commands.VerifyKyc;
using IdentityService.Application.DTOs;
using IdentityService.Application.Queries.GetAllCustomers;
using IdentityService.Application.Queries.GetCustomer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var customers = await mediator.Send(new GetAllCustomersQuery(), ct);
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var customer = await mediator.Send(new GetCustomerQuery(id), ct);
        return Ok(customer);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest req, CancellationToken ct)
    {
        var customer = await mediator.Send(
            new RegisterCustomerCommand(req.FirstName, req.LastName, req.Email, req.Phone), ct);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        var customer = await mediator.Send(
            new UpdateCustomerCommand(id, req.FirstName, req.LastName, req.Phone), ct);
        return Ok(customer);
    }

    [HttpPost("{id:guid}/kyc/verify")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyKyc(Guid id, [FromBody] VerifyKycRequest req, CancellationToken ct)
    {
        var customer = await mediator.Send(new VerifyKycCommand(id, req.NationalId), ct);
        return Ok(customer);
    }
}
