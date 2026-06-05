using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Queries.GetPayment;

public record GetPaymentQuery(Guid PaymentId) : IRequest<PaymentDto>;

public class GetPaymentQueryHandler(IPaymentRepository repository) : IRequestHandler<GetPaymentQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentQuery query, CancellationToken ct)
    {
        var payment = await repository.GetByIdAsync(query.PaymentId, ct)
            ?? throw new PaymentNotFoundException(query.PaymentId);

        return payment.ToDto();
    }
}
