using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.UseCases;

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
        services.AddScoped<IHashingService>(sp => sp.GetRequiredService<IHashingServiceFactory>().CreateAsync().Result);
        services.AddTransient<IInteractionsFilterFactory, InteractionsFilterFactory>();
        services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();

        services.AddSingleton<ITasksStatusesCache, TasksStatusesCache>();

        services.AddScoped<ISecretRotationServiceFactory, SecretRotatorFactory>();
        services.AddScoped<ISecretRotationService>(sp => sp.GetRequiredService<ISecretRotationServiceFactory>().CreateSecretRotator());

        services.AddScoped<IAuthTester>(sp =>
        {
            var connectorInforProvider = sp.GetRequiredService<IConnectorInfoProvider>();
            var connectorInfo = connectorInforProvider.Get();

            var connectorTypes = sp.GetRequiredService<IConnectorTypesCollection>();
            var dataSourceKey = connectorTypes[connectorInfo.Type].GetKeyOf<IAuthTester>();

            return sp.GetRequiredKeyedService<IAuthTester>(dataSourceKey);
        });

        services.AddScoped<IOAuthService>(sp =>
        {
            var connectorInforProvider = sp.GetRequiredService<IConnectorInfoProvider>();
            var connectorInfo = connectorInforProvider.Get();

            var connectorTypes = sp.GetRequiredService<IConnectorTypesCollection>();
            var dataSourceKey = connectorTypes[connectorInfo.Type].GetKeyOf<IOAuthService>();

            return sp.GetRequiredKeyedService<IOAuthService>(dataSourceKey);
        });

        services.AddScoped<IDataSource>(sp =>
        {
            var connectorInforProvider = sp.GetRequiredService<IConnectorInfoProvider>();
            var connectorInfo = connectorInforProvider.Get();

            var connectorTypes = sp.GetRequiredService<IConnectorTypesCollection>();
            var dataSourceKey = connectorTypes[connectorInfo.Type].GetKeyOf<IDataSource>();

            return sp.GetRequiredKeyedService<IDataSource>(dataSourceKey);
        });

        services.AddScoped<IStatusLogger, StatusLogger>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddTransient<ICapabilitiesService, CapabilitiesService>();

        services.AddUseCasesHandling();

        return services;
    }
}