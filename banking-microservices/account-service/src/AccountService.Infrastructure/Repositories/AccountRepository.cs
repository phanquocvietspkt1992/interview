using AccountService.Application.Common;
using AccountService.Domain.Entities;
using AccountService.Domain.Repositories;
using AccountService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repositories;

/// <summary>
/// Concrete EF Core implementation of IAccountRepository.
///
/// Key pattern: after every write operation we:
///   1. SaveChanges() — commits to DB
///   2. Dispatch domain events — notifies other services via the message bus
///
/// Why after SaveChanges()?
///   If the DB commit fails, we don't want to send events for something that didn't happen.
/// </summary>
public class AccountRepository(
    AccountDbContext db,
    IEventPublisher eventPublisher
) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Account?> GetByAccountNumberAsync(string number, CancellationToken ct = default)
        => db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == number, ct);

    public Task<List<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync(ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
    {
        await db.Accounts.AddAsync(account, ct);
        await SaveAndDispatchEventsAsync(account, ct);
    }

    // ── Private ────────────────────────────────────────────────────────────

    private async Task SaveAndDispatchEventsAsync(Account account, CancellationToken ct)
    {
        // Collect events BEFORE saving (EF tracking might clear state)
        var events = account.DomainEvents.ToList();

        await db.SaveChangesAsync(ct);      // 1. Commit to DB first

        // 2. Dispatch events — other services (notification, audit, fraud) will react
        foreach (var domainEvent in events)
            await eventPublisher.PublishAsync(domainEvent, ct);

        account.ClearDomainEvents();
    }
}
