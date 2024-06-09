using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHcpKeyVault(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        services.Configure<HcpVaultConfig>(configurationSection);

        services.AddTransient<HcpVaultClient>();
        services.AddScoped<ISecretRepository, HcpVaultClient>();
        services.AddTransient<ISecretRepositoryFactory, HcpVaultClientFactory>();


        healthCheckBuilder.AddCheck<HcpVaultHealthCheck>("Key-Vault", HealthStatus.Unhealthy, [], TimeSpan.FromSeconds(30));

        return services;
    }
}