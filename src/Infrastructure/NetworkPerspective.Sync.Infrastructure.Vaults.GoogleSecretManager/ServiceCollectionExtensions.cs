using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleSecretManager(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleSecretManagerConfig>(configuration);
        services.AddSingleton<IVault, GoogleSecretManagerClient>();

        return services;
    }
}