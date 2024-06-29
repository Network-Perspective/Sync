using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecretRotationScheduler(this IServiceCollection services, IConfigurationSection configuration)
    {
        services.Configure<SecretRotationSchedulerConfig>(configuration);
        services.AddHostedService<SecretRotationScheduler>();

        return services;
    }
}