using IdentityService.Application.Common;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Messaging;
using IdentityService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IdentityDb")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEventPublisher, LoggingEventPublisher>();

        return services;
    }
}
