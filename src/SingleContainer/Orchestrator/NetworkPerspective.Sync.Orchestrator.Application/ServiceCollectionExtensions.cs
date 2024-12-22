using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Scheduler;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config, string dbConnectionString)
    {
        services.AddScheduler(config.GetSection("Scheduler"), dbConnectionString);

        services.AddTransient<IClock, Clock>();

        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IWorkersService, WorkersService>();
        services.AddTransient<IConnectorsService, ConnectorsService>();
        services.AddTransient<ISyncHistoryService, SyncHistoryService>();
        services.AddTransient<IStatusLogger, StatusLogger>();
        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IStatusService, StatusService>();

        services.AddSingleton<IConnectionsLookupTable, ConnectionsLookupTable>();

        return services;
    }
}