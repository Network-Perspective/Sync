using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExternalAzureKeyVault(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthCheckBuilder)
    {


        return services;
    }
}