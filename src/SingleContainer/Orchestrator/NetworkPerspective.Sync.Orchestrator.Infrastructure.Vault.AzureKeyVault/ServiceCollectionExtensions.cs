using System;

using HealthChecks.AzureKeyVault;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.AzureKeyVault;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.AzureKeyVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureKeyVault(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        services.Configure<AzureKeyVaultConfig>(configurationSection);

        var tokenCreadentials = TokenCredentialFactory.Create();

        services.AddSingleton(tokenCreadentials);
        services.AddScoped<IVault, AzureKeyVaultClient>();

        healthCheckBuilder
            .AddAzureKeyVault(KeyVaultServiceUriFactory, tokenCreadentials, SetupChecks, "Key-Vault", HealthStatus.Unhealthy, [], TimeSpan.FromSeconds(10));

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