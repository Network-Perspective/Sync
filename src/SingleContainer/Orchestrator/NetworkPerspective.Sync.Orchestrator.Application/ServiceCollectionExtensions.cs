using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<IClock, Clock>();

        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IWorkersService, WorkersService>();

        services.AddSingleton<IConnectionsLookupTable, ConnectionsLookupTable>();

        return services;
    }
}