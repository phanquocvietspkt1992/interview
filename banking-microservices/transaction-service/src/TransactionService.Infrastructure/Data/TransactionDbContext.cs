using MassTransit;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Sagas;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Outbox.OutboxMessage> OutboxMessages => Set<Outbox.OutboxMessage>();

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

        modelBuilder.Entity<Outbox.OutboxMessage>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.EventType).IsRequired().HasMaxLength(512);
            entity.Property(o => o.Payload).IsRequired();
            entity.Property(o => o.Error).HasMaxLength(2000);
            entity.HasIndex(o => new { o.ProcessedAt, o.RetryCount });
        });

        modelBuilder.Entity<TransferSagaState>(entity =>
        {
            entity.ToTable("TransferSagaStates");
            entity.HasKey(s => s.CorrelationId);
            entity.Property(s => s.CurrentState).HasMaxLength(64).IsRequired();
            entity.Property(s => s.FailureReason).HasMaxLength(500);
            entity.Property(s => s.Amount).HasPrecision(18, 4);
            entity.HasIndex(s => s.TransactionId);
        });
    }
}
