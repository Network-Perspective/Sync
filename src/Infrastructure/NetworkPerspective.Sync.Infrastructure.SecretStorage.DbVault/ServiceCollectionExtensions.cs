using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.DbVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbSecretStorage(this IServiceCollection services, IConfigurationSection configuration, IHealthChecksBuilder healthChecksBuilder)
    {
        services.Configure<DbSecretRepositoryConfig>(configuration);

        services.AddTransient<DbSecretRepositoryClient>();
        services.AddScoped<ISecretRepository, DbSecretRepositoryClient>();
        services.AddTransient<ISecretRepositoryFactory, DbSecretRepositoryClientFactory>();

        healthChecksBuilder.AddCheck<DbSecretRepositoryHealthCheck>("Db Key-Vault", HealthStatus.Unhealthy, [], TimeSpan.FromSeconds(30));

        return services;
    }
}