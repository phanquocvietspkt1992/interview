using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Common;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Repositories;

namespace NotificationService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Cassandra ──────────────────────────────────────────────────────
        // NotificationService uses Cassandra: optimized for write-heavy time-series data.
        // Millions of notifications per day with sub-millisecond write latency.
        services.AddSingleton<CassandraContext>();
        services.AddScoped<INotificationRepository, NotificationCassandraRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();

        // ── Kafka Consumer (BackgroundService) ─────────────────────────────
        // Consumes from Kafka: can replay from any offset, survives service restarts
        services.AddHostedService<KafkaNotificationConsumer>();

        // ── MassTransit for RabbitMQ (if needed for direct commands) ───────
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq__Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq__Username"] ?? "guest");
                    h.Password(configuration["RabbitMq__Password"] ?? "guest");
                });
            });
        });

        return services;
    }
}
