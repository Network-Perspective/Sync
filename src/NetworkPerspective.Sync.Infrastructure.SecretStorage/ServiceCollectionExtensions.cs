using System;

using HealthChecks.AzureKeyVault;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbDataProtection(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<DbDataProtectionConfig>(configurationSection);
            return services;
        }

        public static IServiceCollection AddSecretStorage(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
        {
            services.Configure<AzureKeyVaultConfig>(configurationSection);

            if (!string.IsNullOrEmpty(configurationSection.GetValue<string>("BaseUrl")))
            {
                var azureCredentials = TokenCredentialFactory.Create();
                healthCheckBuilder
                    .AddAzureKeyVault(KeyVaultServiceUriFactory, azureCredentials, SetupChecks, "Key-Vault", HealthStatus.Unhealthy, Array.Empty<string>(), TimeSpan.FromSeconds(10));
            }
            services.AddSingleton(TokenCredentialFactory.Create());
            services.AddTransient<ISecretRepositoryFactory, AzureKeyVaultClientFactory>();
            services.AddTransient<DbSecretRepositoryClient>();

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
}