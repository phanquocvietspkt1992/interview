using AccountService.Application.Common;
using AccountService.Domain.Repositories;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Messaging.Consumers;
using AccountService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Oracle Database ────────────────────────────────────────────────
        // AccountService uses Oracle XE — polyglot persistence demo.
        // Oracle is the dominant DB in enterprise banking (SWIFT, COBOL migrations).
        services.AddDbContext<AccountDbContext>(options =>
            options.UseOracle(configuration.GetConnectionString("AccountDb")));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();

        // ── MassTransit + RabbitMQ ─────────────────────────────────────────
        services.AddMassTransit(x =>
        {
            x.AddConsumer<DebitAccountConsumer>();
            x.AddConsumer<CreditAccountConsumer>();
            x.AddConsumer<ReverseDebitConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq__Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq__Username"] ?? "guest");
                    h.Password(configuration["RabbitMq__Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("account-debit-queue", e =>
                    e.ConfigureConsumer<DebitAccountConsumer>(ctx));

                cfg.ReceiveEndpoint("account-credit-queue", e =>
                    e.ConfigureConsumer<CreditAccountConsumer>(ctx));

                cfg.ReceiveEndpoint("account-reverse-debit-queue", e =>
                    e.ConfigureConsumer<ReverseDebitConsumer>(ctx));
            });
        });

        return services;
    }
}
