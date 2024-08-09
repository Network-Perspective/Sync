using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;

namespace NetworkPerspective.Sync.Worker;

internal static class ServiceCollectionExtensions
{
    private const string AzureKeyVaultConfigSection = "AzureKeyVault";
    private const string ExternalAzureKeyVaultConfigSection = "ExternalAzureKeyVault";
    private const string HcpVaultConfigSection = "HcpVault";

    public static IServiceCollection AddVault(this IServiceCollection services, IConfigurationSection configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        var configurationSections = configuration.GetChildren();

        var containsAzureKeyVault = configurationSections.Any(x => x.Key == AzureKeyVaultConfigSection);
        var containsExternalAzureKeyVault = configurationSections.Any(x => x.Key == ExternalAzureKeyVaultConfigSection);
        var containsHcpVault = configurationSections.Any(x => x.Key == HcpVaultConfigSection);

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configuration.GetSection(AzureKeyVaultConfigSection), healthChecksBuilder);
        else if (containsExternalAzureKeyVault)
            services.AddExternalAzureKeyVault(configuration.GetSection(ExternalAzureKeyVaultConfigSection), healthChecksBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configuration.GetSection(AzureKeyVaultConfigSection), healthChecksBuilder);
        else
            throw new InvalidVaultConfigurationException("Missing Vault configuration. At least one secret storage needs to be configured");

        return services;
    }

    public static IServiceCollection AddSecretRepositoryClient(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        var azSection = configurationSection.GetSection(AzureKeyVaultConfigSection);
        var hcpSection = configurationSection.GetSection(HcpVaultConfigSection);

        var containsAzureKeyVault = azSection.GetChildren().FirstOrDefault(x => x.Key == "BaseUrl") is not null;
        var containsHcpVault = hcpSection.GetChildren().FirstOrDefault(x => x.Key == "BaseUrl") is not null;
        var containsDbVault = hcpSection.GetChildren().FirstOrDefault(x => x.Key == "SecretsPath") is not null;

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configurationSection.GetSection(AzureKeyVaultConfigSection), healthCheckBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configurationSection.GetSection(AzureKeyVaultConfigSection), healthCheckBuilder);
        else
            throw new VaultException("At least one secret storage needs to be configured");

        return services;
    }






    public static IServiceCollection RemoveHttpClientLogging(this IServiceCollection services)
        => services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

}