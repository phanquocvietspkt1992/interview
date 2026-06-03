using banking_monolithic.Data;
using banking_monolithic.Data;
using banking_monolithic.Models;
using Microsoft.EntityFrameworkCore;
namespace banking_monolithic.Repositories
{
    public class TransactionRepository(BankDbContext db)
    {
        public async Task<List<Transaction>> GetAllAsync()
        {
            return await db.Transactions.ToListAsync();
        }
        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            return await db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task AddAsync(Transaction transaction)
        {
            await db.Transactions.AddAsync(transaction);
        }
        public async Task SaveAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
