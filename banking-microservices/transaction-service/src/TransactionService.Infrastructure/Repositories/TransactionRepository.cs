using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Common;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Repositories;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class TransactionRepository(TransactionDbContext db, IEventPublisher eventPublisher) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<List<Transaction>> GetByAccountIdAsync(Guid accountId, int page, int pageSize, CancellationToken ct)
        => db.Transactions
             .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
             .OrderByDescending(t => t.CreatedAt)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync(ct);

    public async Task AddAsync(Transaction transaction, CancellationToken ct)
    {
        await db.Transactions.AddAsync(transaction, ct);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(transaction, ct);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken ct)
    {
        db.Transactions.Update(transaction);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(transaction, ct);
    }

    private async Task DispatchEventsAsync(Transaction transaction, CancellationToken ct)
    {
        foreach (var e in transaction.DomainEvents)
            await eventPublisher.PublishAsync(e, ct);
        transaction.ClearDomainEvents();
    }
}
