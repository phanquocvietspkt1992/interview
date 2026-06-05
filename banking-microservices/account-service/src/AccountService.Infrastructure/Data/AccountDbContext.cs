using AccountService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Data;

/// <summary>
/// This service's OWN database — one of the core microservice rules.
/// No other service touches this DB directly.
/// </summary>
public class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.AccountNumber)
                  .IsRequired()
                  .HasMaxLength(30);

            entity.HasIndex(a => a.AccountNumber)
                  .IsUnique();

            entity.Property(a => a.Balance)
                  .HasPrecision(18, 4);

            entity.Property(a => a.Type)
                  .HasConversion<string>();    // Store enum as "Checking", "Savings", etc.

            entity.Property(a => a.Status)
                  .HasConversion<string>();

            // Ignore the in-memory domain events collection — not persisted
            entity.Ignore(a => a.DomainEvents);
        });
    }
}
