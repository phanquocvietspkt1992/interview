using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Outbox;

/// <summary>
/// Polls the OutboxMessages table every 5 seconds and publishes unpublished messages via MassTransit.
/// Guarantees at-least-once delivery: messages are written atomically with the business transaction,
/// and this processor retries on transient failures.
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled error in OutboxProcessor loop");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var messageType = Type.GetType(message.EventType)
                    ?? throw new InvalidOperationException($"Cannot resolve type: {message.EventType}");

                var payload = JsonSerializer.Deserialize(message.Payload, messageType)
                    ?? throw new InvalidOperationException($"Cannot deserialize payload for type: {message.EventType}");

                await publisher.Publish(payload, messageType, ct);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;

                logger.LogDebug("Published outbox message {Id} of type {Type}", message.Id, messageType.Name);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogWarning(ex, "Failed to publish outbox message {Id} (attempt {Attempt})",
                    message.Id, message.RetryCount);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
