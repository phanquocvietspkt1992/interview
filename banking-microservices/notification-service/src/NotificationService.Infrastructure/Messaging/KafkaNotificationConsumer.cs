using System.Text.Json;
using BuildingBlocks.Messaging.IntegrationEvents;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Consumes audit events from Kafka and creates notifications in Cassandra.
///
/// Why Kafka here (not RabbitMQ)?
/// - Kafka retains messages for configurable retention — late consumers can replay
/// - Notification service can be down for hours and catch up when it restarts
/// - Multiple services can consume the same audit topic independently (fan-out)
/// - RabbitMQ queues are deleted when all consumers process a message
///
/// Consumer group: "notification-service-group" ensures exactly-one consumption per partition
/// </summary>
public sealed class KafkaNotificationConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<KafkaNotificationConsumer> logger) : BackgroundService
{
    private static readonly string[] Topics = ["transaction-completed", "transaction-failed"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka__BootstrapServers"] ?? "localhost:9092",
            GroupId = "notification-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<Null, string>(config).Build();
        consumer.Subscribe(Topics);
        logger.LogInformation("KafkaNotificationConsumer subscribed to: {Topics}", string.Join(", ", Topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(200));
                if (result is null) continue;

                await ProcessMessageAsync(result.Topic, result.Message.Value, stoppingToken);
                consumer.Commit(result); // manual commit — at-least-once delivery
            }
            catch (OperationCanceledException) { break; }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Kafka message");
                await Task.Delay(1000, stoppingToken);
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(string topic, string json, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        Notification notification = topic switch
        {
            "transaction-completed" => CreateCompletedNotification(json),
            "transaction-failed" => CreateFailedNotification(json),
            _ => throw new InvalidOperationException($"Unknown topic: {topic}")
        };

        await repo.AddAsync(notification, ct);
        logger.LogInformation("Created notification for topic {Topic}, id {NotificationId}", topic, notification.Id);
    }

    private static Notification CreateCompletedNotification(string json)
    {
        var evt = JsonSerializer.Deserialize<TransactionCompletedAuditEvent>(json)!;
        return Notification.Create(
            evt.FromAccountId,
            NotificationChannel.InApp,
            "Transfer Completed",
            $"Your transfer of {evt.Amount:C} has been completed successfully. Reference: {evt.TransactionId}",
            evt.FromAccountId.ToString());
    }

    private static Notification CreateFailedNotification(string json)
    {
        var evt = JsonSerializer.Deserialize<TransactionFailedAuditEvent>(json)!;
        return Notification.Create(
            evt.TransactionId, // using transactionId as a proxy for customerId in this demo
            NotificationChannel.InApp,
            "Transfer Failed",
            $"Your transfer failed: {evt.Reason}. Please contact support if this is unexpected.",
            evt.TransactionId.ToString());
    }
}
