using MassTransit;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Common;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Messaging.Consumers;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── CosmosDB ───────────────────────────────────────────────────────
        // PaymentService uses CosmosDB: global distribution, multi-region writes,
        // schema flexibility for evolving payment fields (SWIFT vs SEPA vs domestic).
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDb")
            ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD3x8mAhwOAiBgbsDjh4==;";

        // HttpClientFactory with SSL bypass for the emulator's self-signed cert.
        // vnext-preview emulator uses a different cert than the legacy emulator.
        // In production this factory is not registered — real CosmosDB uses valid TLS.
        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        services.AddSingleton(_ => new CosmosClient(cosmosConnectionString, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            },
            ConnectionMode = ConnectionMode.Gateway, // required for emulator
            HttpClientFactory = () => new HttpClient(httpHandler),
        }));

        services.AddSingleton<CosmosDbContext>();
        services.AddScoped<IPaymentRepository, PaymentCosmosRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();

        // ── MassTransit + RabbitMQ ─────────────────────────────────────────
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ProcessPaymentConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq__Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq__Username"] ?? "guest");
                    h.Password(configuration["RabbitMq__Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("payment-process-queue", e =>
                    e.ConfigureConsumer<ProcessPaymentConsumer>(ctx));
            });
        });

        return services;
    }
}
