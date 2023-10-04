using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Scheduler;

using Quartz;

namespace NetworkPerspective.Sync.Application.Scheduler
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSecretRotationScheduler(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<SecretRotationConfig>(configurationSection);
            services.AddTransient<ISecretRotationScheduler, SecretRotationScheduler>();
            return services;
        }

        public static IApplicationBuilder UseSecretRotationScheduler(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<ISecretRotationScheduler>().ScheduleSecretsRotation();
            return app;
        }

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

                if (schedulerConfig.UsePersistentStore)
                {
                    q.UsePersistentStore(store =>
                    {
                        store.UseProperties = true;
                        store.UseJsonSerializer();
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