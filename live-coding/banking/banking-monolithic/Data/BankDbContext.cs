using banking_monolithic.Models;
using Microsoft.EntityFrameworkCore;

namespace banking_monolithic.Data
{
    public class BankDbContext(DbContextOptions<BankDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Customer>()
                .HasMany(c => c.Accounts)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId);

            model.Entity<Account>()
                .HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId);

            model.Entity<Account>()
                  .Property(a => a.Balance).HasPrecision(18, 2);
        }
    }
}
