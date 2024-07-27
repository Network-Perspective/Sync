using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Orchestrator.MicrosoftAuth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicrosoftAuth(this IServiceCollection services)
    {
        services.AddScoped<IMicrosoftAuthService, MicrosoftAuthService>();

        services.AddMemoryCache();

        return services;
    }
}