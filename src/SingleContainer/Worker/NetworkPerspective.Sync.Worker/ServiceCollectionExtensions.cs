using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

namespace NetworkPerspective.Sync.Worker;

internal static class ServiceCollectionExtensions
{
    private const string AzureKeyVaultConfigSection = "AzureKeyVault";
    private const string HcpVaultConfigSection = "HcpVault";

    public static IServiceCollection AddVault(this IServiceCollection services, IConfigurationSection configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        var configurationSections = configuration.GetChildren();
        var containsAzureKeyVault = configurationSections.Any(x => x.Key == AzureKeyVaultConfigSection);
        var containsHcpVault = configurationSections.Any(x => x.Key == HcpVaultConfigSection);

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configuration.GetSection(AzureKeyVaultConfigSection), healthChecksBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configuration.GetSection(AzureKeyVaultConfigSection), healthChecksBuilder);
        else
            throw new SecretStorageException("At least one secret storage needs to be configured");

        return services;
    }

    public static IServiceCollection RemoveHttpClientLogging(this IServiceCollection services)
        => services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

}