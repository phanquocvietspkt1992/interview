using AccountService.Domain.Entities;

namespace AccountService.Domain.Repositories;

/// <summary>
/// Repository interface lives in Domain — it defines WHAT we need, not HOW.
/// The concrete implementation (EF Core, Dapper, etc.) is in Infrastructure.
/// This is the Dependency Inversion Principle: Domain depends on abstractions, not EF Core.
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByAccountNumberAsync(string number, CancellationToken ct = default);
    Task<List<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);

    // No Delete — accounts are closed (soft-delete via Status), never hard-deleted.
}
