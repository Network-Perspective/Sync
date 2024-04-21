using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Connector.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectorApplication(this IServiceCollection services)
    {
        return services;
    }
}
