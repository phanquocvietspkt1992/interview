using banking_monolithic.Data;
using banking_monolithic.Data;
using banking_monolithic.Models;
using Microsoft.EntityFrameworkCore;

namespace banking_monolithic.Repositories
{
    public class PaymentRepository(BankDbContext db)
    {
        public async Task<List<Payment>> GetAllAsync()
        {
            return await db.Payments.ToListAsync();
        }
        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await db.Payments.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AddAsync(Payment payment)
        {
            await db.Payments.AddAsync(payment);
        }
        public async Task SaveAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
