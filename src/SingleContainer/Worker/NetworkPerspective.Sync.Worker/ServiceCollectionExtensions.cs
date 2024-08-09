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
    public static IServiceCollection AddVault(this IServiceCollection services, IConfigurationSection configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        const string azureKeyVaultConfigSection = "AzureKeyVault";
        const string hcpVaultConfigSection = "HcpVault";

        var configurationSections = configuration.GetChildren();

        var containsAzureKeyVault = configurationSections.Any(x => x.Key == azureKeyVaultConfigSection);
        var containsHcpVault = configurationSections.Any(x => x.Key == hcpVaultConfigSection);

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configuration.GetSection(azureKeyVaultConfigSection), healthChecksBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configuration.GetSection(azureKeyVaultConfigSection), healthChecksBuilder);
        else
            throw new InvalidVaultConfigurationException("Missing Vault configuration. At least one secret storage needs to be configured");

        services.AddExternalKeyVaultIfApplicable(configuration);

        return services;
    }

    private static IServiceCollection AddExternalKeyVaultIfApplicable(this IServiceCollection services, IConfigurationSection configuration)
    {
        const string externalAzureKeyVaultConfigSection = "ExternalAzureKeyVault";

        var configurationSections = configuration.GetChildren();

        var containsExternalAzureKeyVault = configurationSections.Any(x => x.Key == externalAzureKeyVaultConfigSection);

        if (containsExternalAzureKeyVault)
            services.AddExternalAzureKeyVault(configuration.GetSection(externalAzureKeyVaultConfigSection));

        return services;
    }

    public static IServiceCollection RemoveHttpClientLogging(this IServiceCollection services)
        => services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
}