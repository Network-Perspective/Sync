using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectorApplication(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.AddScoped<ConnectorInfoProvider>();
        services.AddScoped<IConnectorInfoProvider>(x => x.GetRequiredService<ConnectorInfoProvider>());
        services.AddScoped<IConnectorInfoInitializer>(x => x.GetRequiredService<ConnectorInfoProvider>());

        services.AddScoped<ICachedSecretRepository, CachedSecretRepository>();

        services.AddTransient<IClock, Clock>();
        services.AddTransient<IInteractionsFilterFactory, InteractionsFilterFactory>();
        services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();

        services.AddSingleton<ITasksStatusesCache, TasksStatusesCache>();

        services.AddScoped<IDataSourceFactory, DataSourceFactory>();

        services.AddScoped<ISyncService, SyncService>();
        services.AddSingleton<ISyncServiceFactory, SyncServiceFactory>();

        services.AddScoped<ISecretRepository, SecretRepositoryStub>();
        services.AddScoped<ISecretRepositoryFactory, SecretRepositoryFactoryStub>();

        return services;
    }
}

public class SecretRepositoryStub : ISecretRepository
{
    public Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
    {
        throw new System.NotImplementedException();
    }
}

public class SecretRepositoryFactoryStub : ISecretRepositoryFactory
{
    public Task<ISecretRepository> CreateAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(new SecretRepositoryStub() as ISecretRepository);
    }

    public ISecretRepository CreateDefault()
    {
        return new SecretRepositoryStub();
    }
}