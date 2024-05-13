using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Stub;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVaultStub(this IServiceCollection services)
    {
        services.AddScoped<IVault, VaultStub>();

        return services;
    }
}
