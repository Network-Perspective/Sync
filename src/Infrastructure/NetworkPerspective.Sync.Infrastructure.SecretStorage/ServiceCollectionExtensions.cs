using System;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage;

public static class ServiceCollectionExtensions
{
    private const string AzureKeyVaultConfigSection = "AzureKeyVault";
    private const string HcpVaultConfigSection = "HcpVault";

    public static IServiceCollection AddSecretRepositoryClient(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        var configurationSections = configurationSection.GetChildren();
        var containsAzureKeyVault = configurationSections.Any(x => x.Key == AzureKeyVaultConfigSection);
        var containsHcpVault = configurationSections.Any(x => x.Key == HcpVaultConfigSection);

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configurationSection.GetSection(AzureKeyVaultConfigSection), healthCheckBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configurationSection.GetSection(AzureKeyVaultConfigSection), healthCheckBuilder);
        else
            throw new SecretStorageException("At least one secret storage needs to be configured");

        return services;
    }
}