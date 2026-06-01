using BankingApp.Data;
using BankingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Repositories;

public class CustomerRepository(BankDbContext db)
{
    public async Task<Customer?> GetByIdAsync(Guid id)
        => await db.Customers.Include(c => c.Accounts).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Customer>> GetAllAsync()
        => await db.Customers.Include(c => c.Accounts).ToListAsync();

    public async Task AddAsync(Customer customer)
        => await db.Customers.AddAsync(customer);

    public async Task SaveAsync()
        => await db.SaveChangesAsync();
}
