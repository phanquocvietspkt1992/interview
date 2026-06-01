using BankingApp.Data;
using BankingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Repositories;

public class AccountRepository(BankDbContext db)
{
    public async Task<Account?> GetByIdAsync(Guid id)
        => await db.Accounts.FindAsync(id);

    public async Task<Account?> GetByNumberAsync(string accountNumber)
        => await db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task<List<Account>> GetByCustomerAsync(Guid customerId)
        => await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync();

    public async Task AddAsync(Account account)
        => await db.Accounts.AddAsync(account);

    public async Task SaveAsync()
        => await db.SaveChangesAsync();
}
