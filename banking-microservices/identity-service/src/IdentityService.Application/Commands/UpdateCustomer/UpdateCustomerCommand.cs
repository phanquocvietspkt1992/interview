using IdentityService.Application.DTOs;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Commands.UpdateCustomer;

public record UpdateCustomerCommand(Guid CustomerId, string FirstName, string LastName, string Phone) : IRequest<CustomerDto>;

public class UpdateCustomerCommandHandler(
    ICustomerRepository repository
) : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(UpdateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await repository.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new CustomerNotFoundException(cmd.CustomerId);

        customer.Update(cmd.FirstName, cmd.LastName, cmd.Phone);
        await repository.UpdateAsync(customer, ct);

        return customer.ToDto();
    }
}
