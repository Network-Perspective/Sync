using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

internal static class RuntimeVaultExtensions
{
    public static IServiceCollection AddRuntimeVault(this IServiceCollection services, IConfigurationSection configuration, IHealthChecksBuilder healthChecks)
    {
        var provider = configuration["Provider"] ?? "AzureKeyVault";
        if (string.Equals(provider, "AzureKeyVault", StringComparison.OrdinalIgnoreCase))
            return services.AddAzureKeyVault(configuration, healthChecks);
        if (string.Equals(provider, "OvhVault", StringComparison.OrdinalIgnoreCase))
            return services.AddOvhKeyVault(configuration.GetSection("OvhVault"), healthChecks);

        throw new InvalidOperationException("Infrastructure:Vault:Provider must be AzureKeyVault or OvhVault.");
    }
}