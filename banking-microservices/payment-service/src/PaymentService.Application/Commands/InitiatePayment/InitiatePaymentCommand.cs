using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    Guid AccountId,
    string ExternalAccountNumber,
    string BeneficiaryName,
    decimal Amount,
    string Currency,
    PaymentNetwork Network,
    string? Description
) : IRequest<PaymentDto>;

public class InitiatePaymentCommandHandler(
    IPaymentRepository repository
) : IRequestHandler<InitiatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(InitiatePaymentCommand cmd, CancellationToken ct)
    {
        var payment = Payment.Initiate(
            cmd.AccountId,
            cmd.ExternalAccountNumber,
            cmd.BeneficiaryName,
            cmd.Amount,
            cmd.Currency,
            cmd.Network,
            cmd.Description);

        // Simulate processing: in production this goes to a payment processor queue.
        payment.Process();
        payment.Complete();

        await repository.AddAsync(payment, ct);
        return payment.ToDto();
    }
}
