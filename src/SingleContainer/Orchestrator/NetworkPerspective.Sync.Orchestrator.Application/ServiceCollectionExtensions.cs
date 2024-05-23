using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<IClock, Clock>();

        services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();
        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IWorkersService, WorkersService>();
        services.AddTransient<IConnectorsService, ConnectorsService>();
        services.AddTransient<ISyncHistoryService, SyncHistoryService>();
        services.AddTransient<IStatusLogger, StatusLogger>();

        services.AddSingleton<IConnectionsLookupTable, ConnectionsLookupTable>();

        return services;
    }
}