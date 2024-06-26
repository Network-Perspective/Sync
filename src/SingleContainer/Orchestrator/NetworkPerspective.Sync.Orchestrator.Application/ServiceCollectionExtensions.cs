using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfigurationSection config, string dbConnectionString)
    {
        services.AddSyncScheduler(config.GetSection("SyncScheduler"), dbConnectionString);
        services.AddSecretRotationScheduler(config.GetSection("SecretRotationScheduler"));

        services.AddTransient<IClock, Clock>();

        services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();
        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IWorkersService, WorkersService>();
        services.AddTransient<IConnectorsService, ConnectorsService>();
        services.AddTransient<ISyncHistoryService, SyncHistoryService>();
        services.AddTransient<IStatusLogger, StatusLogger>();
        services.AddTransient<ITokenService, TokenService>();

        services.AddSingleton<IConnectionsLookupTable, ConnectionsLookupTable>();

        return services;
    }
}