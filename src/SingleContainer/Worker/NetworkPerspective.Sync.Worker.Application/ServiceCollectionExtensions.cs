using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectorApplication(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.AddScoped<ConnectorInfoProvider>();
        services.AddScoped<IConnectorInfoProvider>(x => x.GetRequiredService<ConnectorInfoProvider>());
        services.AddScoped<IConnectorInfoInitializer>(x => x.GetRequiredService<ConnectorInfoProvider>());

        services.AddScoped<ISyncContextAccessor, SyncContextAccessor>();

        services.AddScoped<ICachedSecretRepository, CachedSecretRepository>();

        services.AddTransient<IClock, Clock>();
        services.AddTransient<IHashingServiceFactory, HashingServiceFactory>();
        services.AddTransient<IInteractionsFilterFactory, InteractionsFilterFactory>();
        services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();

        services.AddSingleton<ITasksStatusesCache, TasksStatusesCache>();

        services.AddScoped<IDataSourceFactory, DataSourceFactory>();
        services.AddScoped<IDataSource>(sp => sp.GetRequiredService<IDataSourceFactory>().CreateDataSource());

        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();

        services.AddScoped<IStatusLogger, StatusLogger>();
        services.AddScoped<ISyncService, SyncService>();

        return services;
    }
}