using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncScheduler(this IServiceCollection services, IConfigurationSection configurationSection, string dbConnectionString)
    {
        var schedulerConfig = new SyncSchedulerConfig();
        configurationSection.Bind(schedulerConfig);
        services.Configure<SyncSchedulerConfig>(configurationSection);

        services.AddTransient<IJobDetailFactory, JobDetailFactory<RemoteSyncJob>>();
        services.AddTransient<ISyncScheduler, SyncScheduler>();

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
            q.StartDelay = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}