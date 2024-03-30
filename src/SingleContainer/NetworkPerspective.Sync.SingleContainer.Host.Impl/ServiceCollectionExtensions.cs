using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl;

public static class ServiceCollectionExtensions
{
    public static void RegisterConnectorHostImpl<T>(this IServiceCollection services) where T : class, IRemoteConnectorClientInternal
    {
        services.AddSingleton<IRemoteConnectorClientInternal, T>();
        services.AddTransient<IRemoteConnectorClient>(s => s.GetRequiredService<IRemoteConnectorClientInternal>());

        services.AddSingleton<IConnectorPool, ConnectorPool>();
        services.AddScoped<IConnectorContextProvider, ConnectorContextProvider>();
        services.AddScoped<IConnectorContext>(s => s.GetRequiredService<IConnectorContextProvider>().Current);
    }
}