using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.HealthChecks;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Init;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IHealthChecksBuilder healthCheckBuilder)
    {
        services.AddSingleton<IDbInitializer, DbInitializer>();
        services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddTransient(x => x.GetRequiredService<IUnitOfWorkFactory>().Create());

        healthCheckBuilder.AddCheck<PersistenceHealthCheck>("Database", HealthStatus.Unhealthy, Array.Empty<string>(), TimeSpan.FromSeconds(10));

        return services;
    }

    public static IServiceCollection AddStartupDbInitializer(this IServiceCollection services)
        => services.AddTransient<IHostedService, DbInitializerHostedService>();
}