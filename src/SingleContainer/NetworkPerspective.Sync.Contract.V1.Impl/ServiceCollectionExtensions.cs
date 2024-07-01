using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrchestratorClient(this IServiceCollection services, IConfigurationSection config)
    {
        services.Configure<WorkerHubClientConfig>(config);

        services.AddSingleton<WorkerHubClient>();
        services.AddSingleton<IWorkerHubClient>(sp => sp.GetRequiredService<WorkerHubClient>());
        services.AddSingleton<IOrchestratorClient>(sp => sp.GetRequiredService<WorkerHubClient>());

        return services;
    }
}