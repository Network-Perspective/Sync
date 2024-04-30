using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework;
using NetworkPerspective.Sync.Framework.Auth;
using NetworkPerspective.Sync.Orchestrator.Hubs;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHub(this IServiceCollection services)
    {
        services
            .AddSingleton<ConnectorHubV1>()
            .AddSignalR();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services
            .AddScoped<NetworkIdProvider>()
            .AddScoped<INetworkIdProvider>(x => x.GetRequiredService<NetworkIdProvider>())
            .AddScoped<INetworkIdInitializer>(x => x.GetRequiredService<NetworkIdProvider>());

        services
            .AddAuthentication(ServiceAuthOptions.DefaultScheme)
            .AddScheme<ServiceAuthOptions, ServiceAuthHandler>(ServiceAuthOptions.DefaultScheme, options => { });

        services
            .AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        services
            .AddTransient<IErrorService, ErrorService>();

        return services;
    }
}
