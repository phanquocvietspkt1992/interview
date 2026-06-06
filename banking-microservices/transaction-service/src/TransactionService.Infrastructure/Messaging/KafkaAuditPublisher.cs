using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TransactionService.Infrastructure.Messaging;

public interface IKafkaAuditPublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken ct = default);
}

/// <summary>
/// Publishes audit events to Kafka for downstream consumers (e.g. NotificationService, analytics).
/// Kafka is used for high-throughput, append-only event streaming — separate from RabbitMQ
/// which handles transactional service-to-service commands.
/// </summary>
public sealed class KafkaAuditPublisher : IKafkaAuditPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaAuditPublisher> _logger;

    public KafkaAuditPublisher(IConfiguration configuration, ILogger<KafkaAuditPublisher> logger)
    {
        _logger = logger;
        var bootstrapServers = configuration["Kafka__BootstrapServers"] ?? "localhost:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader,
            EnableIdempotence = false,
            MessageTimeoutMs = 5000,
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<Null, string> { Value = json };

        try
        {
            var result = await _producer.ProduceAsync(topic, kafkaMessage, ct);
            _logger.LogDebug("Published to Kafka topic {Topic} partition {Partition} offset {Offset}",
                topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Failed to publish message to Kafka topic {Topic}", topic);
            throw;
        }
    }

    public void Dispose() => _producer.Dispose();
}
