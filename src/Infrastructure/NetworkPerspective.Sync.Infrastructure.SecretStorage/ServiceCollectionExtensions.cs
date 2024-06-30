using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.DbVault;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage;

public static class ServiceCollectionExtensions
{
    private const string AzureKeyVaultConfigSection = "AzureKeyVault";
    private const string HcpVaultConfigSection = "HcpVault";
    private const string DbVaultConfigSection = "DataProtection";

    public static IServiceCollection AddSecretRepositoryClient(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {
        var azSection = configurationSection.GetSection(AzureKeyVaultConfigSection);
        var hcpSection = configurationSection.GetSection(HcpVaultConfigSection);
        var dbSection = configurationSection.GetSection(DbVaultConfigSection);

        var containsAzureKeyVault = azSection.GetChildren().FirstOrDefault(x => x.Key == "BaseUrl") is not null;
        var containsHcpVault = hcpSection.GetChildren().FirstOrDefault(x => x.Key == "BaseUrl") is not null;
        var containsDbVault = hcpSection.GetChildren().FirstOrDefault(x => x.Key == "SecretsPath") is not null;

        if (containsAzureKeyVault)
            services.AddAzureKeyVault(configurationSection.GetSection(AzureKeyVaultConfigSection), healthCheckBuilder);
        else if (containsHcpVault)
            services.AddHcpKeyVault(configurationSection.GetSection(AzureKeyVaultConfigSection), healthCheckBuilder);
        else if (containsDbVault)
            services.AddDbSecretStorage(configurationSection.GetSection(DbVaultConfigSection), healthCheckBuilder);
        else
            throw new SecretStorageException("At least one secret storage needs to be configured");

        return services;
    }
}