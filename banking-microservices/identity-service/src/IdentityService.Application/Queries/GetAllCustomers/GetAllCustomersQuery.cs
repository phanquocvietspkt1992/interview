using IdentityService.Application.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Queries.GetAllCustomers;

public record GetAllCustomersQuery : IRequest<List<CustomerDto>>;

public class GetAllCustomersQueryHandler(
    ICustomerRepository repository
) : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery query, CancellationToken ct)
    {
        var customers = await repository.GetAllAsync(ct);
        return customers.Select(c => c.ToDto()).ToList();
    }
}
