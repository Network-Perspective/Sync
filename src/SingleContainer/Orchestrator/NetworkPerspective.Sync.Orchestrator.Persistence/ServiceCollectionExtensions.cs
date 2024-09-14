using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Persistence.HealthChecks;
using NetworkPerspective.Sync.Orchestrator.Persistence.Init;

namespace NetworkPerspective.Sync.Orchestrator.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IHealthChecksBuilder healthCheckBuilder)
    {
        services.AddSingleton<IDbInitializer, DbInitializer>();
        services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddScoped(x => x.GetRequiredService<IUnitOfWorkFactory>().Create());

        healthCheckBuilder.AddCheck<PersistenceHealthCheck>("Database", HealthStatus.Unhealthy, Enumerable.Empty<string>(), TimeSpan.FromSeconds(10));

        return services;
    }

    public static IServiceCollection AddStartupDbInitializer(this IServiceCollection services)
        => services.AddTransient<IHostedService, DbInitializerHostedService>();
}