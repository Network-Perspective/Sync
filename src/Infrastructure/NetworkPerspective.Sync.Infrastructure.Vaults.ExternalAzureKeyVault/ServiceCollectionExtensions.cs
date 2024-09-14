using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExternalAzureKeyVault(this IServiceCollection services, IConfiguration configurationSection)
    {
        // TODO: If ever we want to support this use case it needs to be tested
        services.Configure<ExternalAzureKeyVaultConfig>(configurationSection);

        services.Decorate<IVault, ExternalAzureKeyVaultClient>();

        return services;
    }
}