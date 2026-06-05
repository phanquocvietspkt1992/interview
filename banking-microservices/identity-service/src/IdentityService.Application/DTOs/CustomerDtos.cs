using IdentityService.Domain.Enums;

namespace IdentityService.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    KycStatus KycStatus,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone
);

public record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Phone
);

public record VerifyKycRequest(string NationalId);

public record RejectKycRequest(string Reason);
