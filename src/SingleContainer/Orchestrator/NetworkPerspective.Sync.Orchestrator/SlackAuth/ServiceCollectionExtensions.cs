using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Orchestrator.SlackAuth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSlackAuth(this IServiceCollection services)
    {
        services.AddScoped<ISlackAuthService, SlackAuthService>();

        return services;
    }
}
