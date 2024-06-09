﻿using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    public static class ServiceCollectionExtensions
    {
        private const string AzureKeyVaultConfigSection = "AzureKeyVault";
        private const string HcpVaultConfigSection = "HcpVault";
        private const string DataProtectionConfigSection = "DataProtection";

        public static IServiceCollection AddSecretRepositoryClient(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
        {
            services.Configure<AzureKeyVaultConfig>(configurationSection.GetSection(AzureKeyVaultConfigSection));
            services.Configure<HcpVaultConfig>(configurationSection.GetSection(HcpVaultConfigSection));
            services.Configure<DbSecretRepositoryConfig>(configurationSection.GetSection(DataProtectionConfigSection));

            services.AddSingleton(TokenCredentialFactory.Create());
            services.AddTransient<DbSecretRepositoryClient>();
            services.AddTransient<HcpVaultClient>();
            services.AddTransient<ISecretRepositoryFactory, SecretRepositoryClientFactory>();
            services.AddTransient<ISecretRepositoryHealthCheckFactory, SecretRepositoryClientFactory>();

            services.AddScoped<ISecretRepository>(sp =>
            {
                var factory = sp.GetRequiredService<ISecretRepositoryFactory>();
                var connectorInfoProvider = sp.GetRequiredService<IConnectorInfoProvider>();
                var connectorService = sp.GetRequiredService<IConnectorService>();

                var connectorInfo = connectorInfoProvider.Get();
                var connector = connectorService.GetAsync<ConnectorProperties>(connectorInfo.Id).Result;
                return factory.Create(connector.Properties.ExternalKeyVaultUri);
            });

            services.AddTransient<HcpVaultHealthCheck>();
            services.AddTransient<DbSecretRepositoryHealthCheck>();
            healthCheckBuilder.Add(new HealthCheckRegistration(
                "SecretRepository",
                sp =>
                {
                    var factory = sp.GetRequiredService<ISecretRepositoryHealthCheckFactory>();
                    return factory.CreateHealthCheck();
                },
                HealthStatus.Unhealthy,
                Array.Empty<string>(),
                TimeSpan.FromSeconds(30))
            );

            return services;
        }
    }
}