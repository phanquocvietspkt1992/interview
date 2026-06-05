using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone
) : IRequest<CustomerDto>;

public class RegisterCustomerCommandHandler(
    ICustomerRepository repository
) : IRequestHandler<RegisterCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(RegisterCustomerCommand cmd, CancellationToken ct)
    {
        var existing = await repository.GetByEmailAsync(cmd.Email, ct);
        if (existing is not null)
            throw new DuplicateEmailException(cmd.Email);

        var customer = Customer.Register(cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone);
        await repository.AddAsync(customer, ct);

        return customer.ToDto();
    }
}
