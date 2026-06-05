using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;

namespace PaymentService.Application;

public static class PaymentMappings
{
    public static PaymentDto ToDto(this Payment p) => new(
        p.Id,
        p.Reference,
        p.AccountId,
        p.ExternalAccountNumber,
        p.BeneficiaryName,
        p.Amount,
        p.Currency,
        p.Network,
        p.Status,
        p.FailureReason,
        p.Description,
        p.CreatedAt,
        p.ProcessedAt
    );
}
