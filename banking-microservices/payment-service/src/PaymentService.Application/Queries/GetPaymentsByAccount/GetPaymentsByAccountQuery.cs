using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Queries.GetPaymentsByAccount;

public record GetPaymentsByAccountQuery(Guid AccountId) : IRequest<List<PaymentDto>>;

public class GetPaymentsByAccountQueryHandler(IPaymentRepository repository)
    : IRequestHandler<GetPaymentsByAccountQuery, List<PaymentDto>>
{
    public async Task<List<PaymentDto>> Handle(GetPaymentsByAccountQuery query, CancellationToken ct)
    {
        var payments = await repository.GetByAccountIdAsync(query.AccountId, ct);
        return payments.Select(p => p.ToDto()).ToList();
    }
}
