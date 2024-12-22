using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.SecretRotation;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScheduler(this IServiceCollection services, IConfiguration configuration, string dbConnectionString)
    {
        var schedulerConfig = new SchedulerConfig();
        configuration.Bind(schedulerConfig);

        services.AddQuartz(q =>
        {
            q.SchedulerId = "scheduler-connector";

            if (schedulerConfig.UsePersistentStore)
            {
                q.UsePersistentStore(store =>
                {
                    store.UseProperties = true;
                    store.UseNewtonsoftJsonSerializer();
                    store.UseSqlServer(db =>
                    {
                        db.ConnectionString = dbConnectionString;
                    });
                });
            }
            else
            {
                q.UseInMemoryStore();
            }

            q.InterruptJobsOnShutdown = true;

            q.UseSimpleTypeLoader();
            q.UseDefaultThreadPool(threadPool =>
            {
                threadPool.MaxConcurrency = 4;
            });
        });

        services.AddQuartzHostedService(q =>
        {
            q.WaitForJobsToComplete = true;
            q.StartDelay = schedulerConfig.StartDelay;
        });

        services
            .AddSyncScheduler(configuration.GetSection("Sync"))
            .AddSecretRotationScheduler(configuration.GetSection("SecretRotation"));

        return services;
    }
}