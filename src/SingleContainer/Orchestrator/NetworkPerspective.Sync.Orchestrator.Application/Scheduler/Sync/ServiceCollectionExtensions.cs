using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SyncSchedulerConfig>(configuration);

        services.AddTransient<IJobDetailFactory, JobDetailFactory<RemoteSyncJob>>();
        services.AddTransient<ISyncScheduler, SyncScheduler>();

        return services;
    }
}