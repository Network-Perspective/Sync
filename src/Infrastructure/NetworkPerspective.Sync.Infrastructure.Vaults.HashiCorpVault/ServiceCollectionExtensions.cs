using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHcpKeyVault(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        services.Configure<HcpVaultConfig>(configurationSection);

        services.AddTransient<HcpVaultClient>();
        services.AddSingleton<IVault, HcpVaultClient>();

        healthCheckBuilder.AddCheck<HcpVaultHealthCheck>("Hcp Key-Vault", HealthStatus.Unhealthy, [], TimeSpan.FromSeconds(30));

        return services;
    }
}