using System;

using HealthChecks.AzureKeyVault;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureKeyVault(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        services.Configure<AzureKeyVaultConfig>(configurationSection);

        var azureCredentials = TokenCredentialFactory.Create();

        services.AddSingleton(azureCredentials);

        services.AddTransient<ISecretRepositoryFactory, AzureKeyVaultClientFactory>();

        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<ISecretRepositoryFactory>();
            var connectorInfoProvider = sp.GetRequiredService<IConnectorInfoProvider>();
            var connectorService = sp.GetRequiredService<IConnectorService>();

            var connectorInfo = connectorInfoProvider.Get();
            var connector = connectorService.GetAsync<ConnectorProperties>(connectorInfo.Id).Result;
            return factory.Create(connector.Properties.ExternalKeyVaultUri);
        });

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