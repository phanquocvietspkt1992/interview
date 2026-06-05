using IdentityService.Application.DTOs;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Queries.GetCustomer;

public record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerDto>;

public class GetCustomerQueryHandler(
    ICustomerRepository repository
) : IRequestHandler<GetCustomerQuery, CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerQuery query, CancellationToken ct)
    {
        var customer = await repository.GetByIdAsync(query.CustomerId, ct)
            ?? throw new CustomerNotFoundException(query.CustomerId);

        return customer.ToDto();
    }
}
