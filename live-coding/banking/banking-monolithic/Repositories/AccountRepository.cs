using banking_monolithic.Data;
using banking_monolithic.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace banking_monolithic.Repositories
{
    public class AccountRepository(BankDbContext db)        
    {
        public async Task<List<Account>> GetAllAsync()
        {
            return await db.Accounts.ToListAsync(); 
        }

        public async Task<Account> GetByIdAsync(Guid id)
        {
            return await db.Accounts.FindAsync(id);
        }

        public async Task AddAsync(Account account)
        {
            await db.Accounts.AddAsync(account);
        }

        public async Task SaveAsync()
        { 
            await db.SaveChangesAsync();
        }

        public async Task<Account?> GetByNumberAsync(string accountNumber)
       => await db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    }
}
