using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs;

public record PaymentDto(
    Guid Id,
    string Reference,
    Guid AccountId,
    string ExternalAccountNumber,
    string BeneficiaryName,
    decimal Amount,
    string Currency,
    PaymentNetwork Network,
    PaymentStatus Status,
    string? FailureReason,
    string? Description,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record InitiatePaymentRequest(
    Guid AccountId,
    string ExternalAccountNumber,
    string BeneficiaryName,
    decimal Amount,
    string Currency,
    PaymentNetwork Network,
    string? Description
);
