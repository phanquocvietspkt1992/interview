using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Reference).IsRequired().HasMaxLength(30);
            entity.HasIndex(t => t.Reference).IsUnique();

            entity.Property(t => t.Amount).HasPrecision(18, 4);
            entity.Property(t => t.Type).HasConversion<string>();
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.FailureReason).HasMaxLength(500);

            entity.HasIndex(t => t.FromAccountId);
            entity.HasIndex(t => t.ToAccountId);

            entity.Ignore(t => t.DomainEvents);
        });
    }
}
