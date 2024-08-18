using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AmazonSecretsManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAmazonSecretsManager(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AmazonSecretsManagerConfig>(configuration);
        services.AddSingleton<IVault, AmazonSecretsManagerClient>();

        return services;
    }
}