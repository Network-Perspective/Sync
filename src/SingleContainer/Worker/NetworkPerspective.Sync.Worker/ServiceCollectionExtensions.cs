using System.Linq;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

using NetworkPerspective.Sync.Infrastructure.Vaults.AmazonSecretsManager;
using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager;
using NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;
using NetworkPerspective.Sync.Worker.ApplicationInsights;

namespace NetworkPerspective.Sync.Worker;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVault(this IServiceCollection services, IConfiguration configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        const string azureKeyVaultConfigSection = "AzureKeyVault";
        const string hcpVaultConfigSection = "HcpVault";
        const string googleSecretManagerConfigSection = "GoogleSecretManager";
        const string amazonSecretsManagerConfigSection = "AmazonSecretsManager";

        var configurationSections = configuration.GetChildren();

        var containsAzureKeyVault = configurationSections.Any(x => x.Key == azureKeyVaultConfigSection);
        var containsHcpVault = configurationSections.Any(x => x.Key == hcpVaultConfigSection);
        var containsGoogleSecretManagerConfigSection = configurationSections.Any(x => x.Key == googleSecretManagerConfigSection);
        var containsAmazonSecretsManager = configurationSections.Any(x => x.Key == amazonSecretsManagerConfigSection);

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configuration.GetSection(azureKeyVaultConfigSection), healthChecksBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configuration.GetSection(azureKeyVaultConfigSection));
        else if (containsGoogleSecretManagerConfigSection)
            services.AddGoogleSecretManager(configuration.GetSection(googleSecretManagerConfigSection));
        else if (containsAmazonSecretsManager)
            services.AddAmazonSecretsManager(configuration.GetSection(amazonSecretsManagerConfigSection));
        else
            throw new InvalidVaultConfigurationException("Missing Vault configuration. At least one secret storage needs to be configured");

        services.AddExternalKeyVaultIfApplicable(configuration);

        return services;
    }

    private static IServiceCollection AddExternalKeyVaultIfApplicable(this IServiceCollection services, IConfiguration configuration)
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

    public static IServiceCollection AddApplicaitonInsights(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ApplicationInsightConfig>()
            .Bind(configuration);

        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddSingleton<ITelemetryInitializer, CloudContextTelemetryInitializer>();

        return services;
    }
}