using LoanService.Application.Common;
using LoanService.Domain.Repositories;
using LoanService.Infrastructure.Data;
using LoanService.Infrastructure.Messaging;
using LoanService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoanService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── MongoDB ────────────────────────────────────────────────────────
        // LoanService uses MongoDB: flexible schema for different loan products,
        // embedded PaymentHistory documents, no schema migrations needed.
        services.AddSingleton<MongoDbContext>();
        services.AddScoped<ILoanRepository, LoanMongoRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();

        // ── MassTransit + RabbitMQ ─────────────────────────────────────────
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
