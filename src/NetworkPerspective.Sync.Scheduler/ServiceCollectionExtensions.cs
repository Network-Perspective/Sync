using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Scheduler;

using Quartz;

namespace NetworkPerspective.Sync.Application.Scheduler
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScheduler(this IServiceCollection services, IConfigurationSection configurationSection, string dbConnectionString)
        {
            var schedulerConfig = new SchedulerConfig();
            configurationSection.Bind(schedulerConfig);
            services.Configure<SchedulerConfig>(configurationSection);

            services.AddTransient<IJobDetailFactory, JobDetailFactory<SyncJob>>();
            services.AddTransient<ISyncScheduler, SyncScheduler>();

            services.AddQuartz(q =>
            {
                q.SchedulerId = "scheduler-connector";
                q.UsePersistentStore(store =>
                {
                    store.UseProperties = true;
                    store.UseJsonSerializer();
                    store.UseSqlServer(db =>
                    {
                        db.ConnectionString = dbConnectionString;
                    });
                });
                q.InterruptJobsOnShutdown = true;
                q.UseMicrosoftDependencyInjectionJobFactory();

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
}