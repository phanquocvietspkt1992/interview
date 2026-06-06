using AccountService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Data;

/// <summary>
/// AccountService uses Oracle XE — demonstrating polyglot persistence.
/// Oracle is common in enterprise banking (COBOL/mainframe migration paths).
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
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(a => a.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasIndex(a => a.CustomerId);
            entity.Ignore(a => a.DomainEvents);
        });
    }
}
