using banking_monolithic.Data;
using banking_monolithic.Data;
using banking_monolithic.Models;
using Microsoft.EntityFrameworkCore;

namespace banking_monolithic.Repositories
{
    public class LoanRepository(BankDbContext db)
    {
        public async Task<List<Loan>> GetAllAsync()
        {
            return await db.Loans.ToListAsync();
        }

        public async Task<Loan?> GetByIdAsync(Guid id)
        {
            return await db.Loans.FirstOrDefaultAsync(l => l.Id == id);
        }
        public async Task AddAsync(Loan loan)
        {
            await db.Loans.AddAsync(loan);
        }   
        public async Task SaveAsync()
        {
            await db.SaveChangesAsync();
        }   

    }
}
