using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Subject).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Body).IsRequired().HasMaxLength(2000);
            entity.Property(n => n.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Channel).HasConversion<string>();
            entity.Property(n => n.Status).HasConversion<string>();
            entity.Property(n => n.FailureReason).HasMaxLength(500);
            entity.HasIndex(n => n.CustomerId);
            entity.Ignore(n => n.DomainEvents);
        });
    }
}
