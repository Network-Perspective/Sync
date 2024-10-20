using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrchestratorClient(this IServiceCollection services, IConfigurationSection config)
    {
        services.Configure<OrchestratorHubClientConfig>(config);

        services.AddSingleton<OrchestartorHubClient>();
        services.AddSingleton<IOrchestratorHubClient>(sp => sp.GetRequiredService<OrchestartorHubClient>());
        services.AddSingleton<IOrchestratorClient>(sp => sp.GetRequiredService<OrchestartorHubClient>());

        return services;
    }
}