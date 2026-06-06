using Cassandra;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Repositories;

/// <summary>
/// Cassandra implementation — uses prepared statements for all operations.
/// Prepared statements are parsed once by Cassandra, reducing latency on repeated calls.
/// </summary>
public class NotificationCassandraRepository(
    CassandraContext context,
    ILogger<NotificationCassandraRepository> logger) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Step 1: look up (customerId, createdAt) from the lookup table
        var lookupResult = await context.Session.ExecuteAsync(
            context.SelectByIdStmt.Bind(id));

        var lookupRow = lookupResult.FirstOrDefault();
        if (lookupRow is null) return null;

        var customerId = lookupRow.GetValue<Guid>("customer_id");
        var createdAt = lookupRow.GetValue<DateTimeOffset>("created_at");

        // Step 2: get the full notification from the main table
        var result = await context.Session.ExecuteAsync(
            new SimpleStatement(
                "SELECT * FROM notifications WHERE customer_id = ? AND created_at = ? AND id = ?",
                customerId, createdAt, id));

        var row = result.FirstOrDefault();
        return row is null ? null : MapToDomain(row);
    }

    public async Task<List<Notification>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var result = await context.Session.ExecuteAsync(
            context.SelectByCustomerStmt.Bind(customerId));

        return result.Select(MapToDomain).ToList();
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        var batch = new BatchStatement();

        // Insert into main notifications table
        batch.Add(context.InsertStmt.Bind(
            notification.CustomerId,
            (DateTimeOffset)notification.CreatedAt,
            notification.Id,
            notification.Channel.ToString(),
            notification.Status.ToString(),
            notification.Subject,
            notification.Body));

        // Insert into lookup table for id-based queries
        batch.Add(new SimpleStatement(
            "INSERT INTO notifications_by_id (id, customer_id, created_at) VALUES (?, ?, ?)",
            notification.Id, notification.CustomerId, (DateTimeOffset)notification.CreatedAt));

        await context.Session.ExecuteAsync(batch);
        logger.LogDebug("Notification {Id} saved to Cassandra", notification.Id);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        DateTimeOffset? sentAt = notification.SentAt.HasValue
            ? (DateTimeOffset)notification.SentAt.Value
            : null;

        await context.Session.ExecuteAsync(context.UpdateStatusStmt.Bind(
            notification.Status.ToString(),
            sentAt,
            notification.CustomerId,
            (DateTimeOffset)notification.CreatedAt,
            notification.Id));
    }

    private static Notification MapToDomain(Row row)
    {
        var channel = Enum.Parse<NotificationChannel>(row.GetValue<string>("channel"));
        var notification = Notification.Create(
            row.GetValue<Guid>("customer_id"),
            channel,
            row.GetValue<string>("subject"),
            row.GetValue<string>("body"),
            "N/A");

        var status = row.GetValue<string>("status");
        if (status == "Sent") notification.MarkSent();
        else if (status == "Failed") notification.MarkFailed("Unknown");

        notification.ClearDomainEvents();
        return notification;
    }
}
