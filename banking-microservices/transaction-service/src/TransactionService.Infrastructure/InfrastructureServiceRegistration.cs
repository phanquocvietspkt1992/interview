using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Common;
using TransactionService.Domain.Repositories;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Messaging;
using TransactionService.Infrastructure.Outbox;
using TransactionService.Infrastructure.Repositories;
using TransactionService.Infrastructure.Sagas;

namespace TransactionService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ───────────────────────────────────────────────────────
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TransactionDb")));

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // ── Kafka ──────────────────────────────────────────────────────────
        services.AddSingleton<IKafkaAuditPublisher, KafkaAuditPublisher>();

        // ── Outbox Processor ───────────────────────────────────────────────
        // Polls OutboxMessages every 5s and publishes via MassTransit
        services.AddHostedService<OutboxProcessor>();

        // ── MassTransit + RabbitMQ ─────────────────────────────────────────
        services.AddMassTransit(x =>
        {
            // Saga state machine stored in SQL Server via EF Core
            x.AddSagaStateMachine<TransferSagaMachine, TransferSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    r.AddDbContext<DbContext, TransactionDbContext>((provider, opts) =>
                        opts.UseSqlServer(configuration.GetConnectionString("TransactionDb")));
                });

            // Terminal state consumers — update Transaction + publish to Kafka
            x.AddConsumer<TransferCompletedConsumer>();
            x.AddConsumer<TransferFailedConsumer>();
            x.AddConsumer<TransferPaymentFailedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq__Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq__Username"] ?? "guest");
                    h.Password(configuration["RabbitMq__Password"] ?? "guest");
                });

                // Saga queue — receives TransferSagaStarted and all response events
                cfg.ReceiveEndpoint("transfer-saga-queue", e =>
                {
                    e.StateMachineSaga<TransferSagaState>(ctx);
                });

                // Completion consumers
                cfg.ReceiveEndpoint("transfer-completed-queue", e =>
                    e.ConfigureConsumer<TransferCompletedConsumer>(ctx));
                cfg.ReceiveEndpoint("transfer-failed-queue", e =>
                    e.ConfigureConsumer<TransferFailedConsumer>(ctx));
                cfg.ReceiveEndpoint("transfer-payment-failed-queue", e =>
                    e.ConfigureConsumer<TransferPaymentFailedConsumer>(ctx));
            });
        });

        return services;
    }
}
