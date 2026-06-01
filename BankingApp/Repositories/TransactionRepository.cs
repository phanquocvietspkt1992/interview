using BankingApp.Data;
using BankingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Repositories;

public class TransactionRepository(BankDbContext db)
{
    public async Task<List<Transaction>> GetByAccountAsync(Guid accountId)
        => await db.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Transaction transaction)
        => await db.Transactions.AddAsync(transaction);

    public async Task SaveAsync()
        => await db.SaveChangesAsync();
}
