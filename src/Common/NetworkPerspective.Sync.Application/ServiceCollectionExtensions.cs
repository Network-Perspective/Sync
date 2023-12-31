﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Application
{
    public static class ServiceCollectionExtensions
    {
        private const string SyncConfigSection = "Sync";
        private const string MiscConfigSection = "Misc";

        public static IServiceCollection AddApplication(this IServiceCollection services, IConfigurationSection config)
        {
            services.Configure<SyncConfig>(config.GetSection(SyncConfigSection));
            services.Configure<MiscConfig>(config.GetSection(MiscConfigSection));

            services.AddTransient<IClock, Clock>();
            services.AddTransient<IHashingServiceFactory, HashingServiceFactory>();
            services.AddTransient<IInteractionsFilterFactory, InteractionsFilterFactory>();
            services.AddTransient<IHashingServiceFactory, HashingServiceFactory>();
            services.AddTransient<IAuthStateKeyFactory, AuthStateKeyFactory>();

            services.AddTransient<ISyncServiceFactory, SyncServiceFactory>();

            services.AddTransient<INetworkService, NetworkService>();
            services.AddTransient<IStatusLoggerFactory, StatusLoggerFactory>();
            services.AddTransient<IStatusService, StatusService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<ISyncHistoryService, SyncHistoryService>();
            services.AddTransient<ISyncContextFactory, SyncContextFactory>();

            services.AddSingleton<ITasksStatusesCache, TasksStatusesCache>();

            return services;
        }
    }
}