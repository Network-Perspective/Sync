﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfigurationSection configSection)
        {
            services.AddTransient<IMicrosoftClientFactory, MicrosoftClientFactory>();
            services.AddSingleton<IDataSourceFactory, MicrosoftFacadeFactory>();
            return services;
        }
    }
}