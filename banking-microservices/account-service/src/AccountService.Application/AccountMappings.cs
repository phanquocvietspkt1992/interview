using AccountService.Application.DTOs;
using AccountService.Domain.Entities;

namespace AccountService.Application;

/// <summary>
/// Extension method to map Domain → DTO.
/// Kept here (Application layer) because the API shouldn't know domain internals.
/// In larger projects, use AutoMapper or Mapster.
/// </summary>
public static class AccountMappings
{
    public static AccountDto ToDto(this Account a) => new(
        a.Id,
        a.AccountNumber,
        a.CustomerId,
        a.Balance,
        a.Type.ToString(),
        a.Status.ToString(),
        a.CreatedAt,
        a.UpdatedAt
    );
}
