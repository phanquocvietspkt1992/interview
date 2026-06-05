using LoanService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanService.Infrastructure.Data;

public class LoanDbContext(DbContextOptions<LoanDbContext> options) : DbContext(options)
{
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.PrincipalAmount).HasPrecision(18, 4);
            entity.Property(l => l.OutstandingBalance).HasPrecision(18, 4);
            entity.Property(l => l.InterestRate).HasPrecision(5, 4);
            entity.Property(l => l.Status).HasConversion<string>();
            entity.Property(l => l.RejectionReason).HasMaxLength(500);
            entity.HasIndex(l => l.CustomerId);
            entity.Ignore(l => l.DomainEvents);
            entity.Ignore(l => l.MonthlyPayment);
        });
    }
}
