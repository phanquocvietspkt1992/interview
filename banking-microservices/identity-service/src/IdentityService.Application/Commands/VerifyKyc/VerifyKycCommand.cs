using IdentityService.Application.DTOs;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Commands.VerifyKyc;

public record VerifyKycCommand(Guid CustomerId, string NationalId) : IRequest<CustomerDto>;

public class VerifyKycCommandHandler(
    ICustomerRepository repository
) : IRequestHandler<VerifyKycCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(VerifyKycCommand cmd, CancellationToken ct)
    {
        var customer = await repository.GetByIdAsync(cmd.CustomerId, ct)
            ?? throw new CustomerNotFoundException(cmd.CustomerId);

        customer.VerifyKyc(cmd.NationalId);
        await repository.UpdateAsync(customer, ct);

        return customer.ToDto();
    }
}
