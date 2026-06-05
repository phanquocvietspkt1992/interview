using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application;

public static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer c) => new(
        c.Id,
        c.FirstName,
        c.LastName,
        c.FullName,
        c.Email,
        c.Phone,
        c.KycStatus,
        c.IsActive,
        c.CreatedAt,
        c.UpdatedAt
    );
}
