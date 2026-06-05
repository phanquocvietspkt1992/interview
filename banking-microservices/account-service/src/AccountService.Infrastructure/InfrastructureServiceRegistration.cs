using AccountService.Application.Common;
using AccountService.Domain.Repositories;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Infrastructure;

/// <summary>
/// Infrastructure DI registration.
/// The API calls this — it knows "I need Infrastructure", but doesn't care about EF Core details.
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AccountDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AccountDb")));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();

        return services;
    }
}
