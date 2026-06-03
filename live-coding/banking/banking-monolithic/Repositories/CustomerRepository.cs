using banking_monolithic.Data;
using banking_monolithic.Models;
using Microsoft.EntityFrameworkCore;

namespace banking_monolithic.Repositories
{
    public class CustomerRepository(BankDbContext db)
    {
        public async Task<List<Customer>> GetAllAsync()
        {
            return await db.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await db.Customers.FindAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            db.Customers.AddAsync(customer);
        }
        public async Task SaveAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
