using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Data;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Reference).IsRequired().HasMaxLength(30);
            entity.HasIndex(p => p.Reference).IsUnique();
            entity.Property(p => p.ExternalAccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(p => p.BeneficiaryName).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Amount).HasPrecision(18, 4);
            entity.Property(p => p.Currency).IsRequired().HasMaxLength(3);
            entity.Property(p => p.Network).HasConversion<string>();
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.FailureReason).HasMaxLength(500);
            entity.HasIndex(p => p.AccountId);
            entity.Ignore(p => p.DomainEvents);
        });
    }
}
