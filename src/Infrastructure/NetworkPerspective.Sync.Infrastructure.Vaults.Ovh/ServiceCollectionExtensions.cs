using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOvhKeyVault(this IServiceCollection services, IConfiguration configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        services.AddOptions<OvhVaultConfig>()
            .Bind(configurationSection)
            .Validate(config => new OvhVaultConfig.Validator("OvhVault").Validate(config).IsValid,
                "Invalid OVH KV2 configuration: require HTTPS /api/<domain UUID>, Token, MountPoint=secret and TestSecretName.")
            .ValidateOnStart();

        services.AddSingleton<OvhVaultClient>();
        services.AddSingleton<IVault>(provider => provider.GetRequiredService<OvhVaultClient>());
        healthCheckBuilder.AddCheck<OvhVaultHealthCheck>("OVH Key-Vault", HealthStatus.Unhealthy, [], TimeSpan.FromSeconds(30));

        return services;
    }
}