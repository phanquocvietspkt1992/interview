using AccountService.Application.Common;
using AccountService.Domain.Entities;
using AccountService.Domain.Repositories;
using AccountService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repositories;

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

    public async Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        db.Accounts.Update(account);
        await SaveAndDispatchEventsAsync(account, ct);
    }

    private async Task SaveAndDispatchEventsAsync(Account account, CancellationToken ct)
    {
        var events = account.DomainEvents.ToList();
        await db.SaveChangesAsync(ct);
        foreach (var domainEvent in events)
            await eventPublisher.PublishAsync(domainEvent, ct);
        account.ClearDomainEvents();
    }
}
