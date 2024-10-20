using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerApplication(this IServiceCollection services, IConfigurationSection configurationSection, IEnumerable<ConnectorType> connectorTypes)
    {
        services.AddSingleton(new ConnectorTypesCollection(connectorTypes) as IConnectorTypesCollection);

        services.AddScoped<ConnectorInfoProvider>();
        services.AddScoped<IConnectorInfoProvider>(x => x.GetRequiredService<ConnectorInfoProvider>());
        services.AddScoped<IConnectorInfoInitializer>(x => x.GetRequiredService<ConnectorInfoProvider>());

        services.AddSingleton<ISyncContextFactory, SyncContextFactory>();
        services.AddScoped<ISyncContextAccessor, SyncContextAccessor>();

        services.AddScoped<ICachedVault, CachedVault>();

        services.AddTransient<IClock, Clock>();
        services.AddTransient<IHashingServiceFactory, HashingServiceFactory>();
        services.AddTransient<IInteractionsFilterFactory, InteractionsFilterFactory>();
        services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();

        services.AddSingleton<ITasksStatusesCache, TasksStatusesCache>();

        services.AddScoped<IDataSourceFactory, DataSourceFactory>();
        services.AddScoped<IDataSource>(sp => sp.GetRequiredService<IDataSourceFactory>().CreateDataSource());

        services.AddSingleton<ISecretRotationContextFactory, SecretRotationContextFactory>();
        services.AddScoped<ISecretRotationContextAccessor, SecretRotationContextAccessor>();
        services.AddScoped<ISecretRotationServiceFactory, SecretRotatorFactory>();
        services.AddScoped<ISecretRotationService>(sp => sp.GetRequiredService<ISecretRotationServiceFactory>().CreateSecretRotator());

        services.AddSingleton<IAuthTesterContextFactory, AuthTesterContextFactory>();
        services.AddScoped<IAuthTesterContextAccessor, AuthTesterContextAccessor>();
        services.AddScoped<IAuthTesterFactory, AuthTesterFactory>();
        services.AddScoped<IAuthTester>(sp => sp.GetRequiredService<IAuthTesterFactory>().CreateAuthTester());


        services.AddScoped<IStatusLogger, StatusLogger>();
        services.AddScoped<ISyncService, SyncService>();

        return services;
    }
}