using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth;
using NetworkPerspective.Sync.Orchestrator.Hubs;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHub(this IServiceCollection services)
    {
        services
            .AddSingleton<WorkerHubV1>()
            .AddSignalR();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
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