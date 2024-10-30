using System;

using HealthChecks.AzureKeyVault;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureKeyVault(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        services.Configure<AzureKeyVaultConfig>(configurationSection);

        var azureCredentials = TokenCredentialFactory.Create();

        services.AddSingleton(azureCredentials);
        services.AddSingleton<IVault, AzureKeyVaultClient>();

        healthCheckBuilder
            .AddAzureKeyVault(KeyVaultServiceUriFactory, azureCredentials, SetupChecks, "Azure Key-Vault", HealthStatus.Unhealthy, [], TimeSpan.FromSeconds(30));

        return services;
    }

    private static Uri KeyVaultServiceUriFactory(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetService<IOptions<AzureKeyVaultConfig>>().Value;

        return new Uri(config.BaseUrl);
    }

    private static void SetupChecks(IServiceProvider serviceProvider, AzureKeyVaultOptions options)
    {
        var config = serviceProvider.GetService<IOptions<AzureKeyVaultConfig>>().Value;

        options.AddSecret(config.TestSecretName);
    }
}