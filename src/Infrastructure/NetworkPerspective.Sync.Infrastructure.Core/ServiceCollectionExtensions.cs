using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Infrastructure.Core.HealthChecks;
using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;

using Polly;

namespace NetworkPerspective.Sync.Infrastructure.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetworkPerspectiveCore(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthChecksBuilder)
        {
            var config = new NetworkPerspectiveCoreConfig();
            configurationSection.Bind(config);
            services.Configure<NetworkPerspectiveCoreConfig>(configurationSection);

            healthChecksBuilder
                .AddCheck<NetworkPerspectiveCoreHealthCheck>("Network-Perspective-Core", HealthStatus.Unhealthy, Array.Empty<string>());

            Action<IHttpClientBuilder> configureHttpClient = httpClientBuilder =>
            {
                httpClientBuilder.ConfigureHttpClient(client => client.BaseAddress = new Uri(config.BaseUrl));

                if (config.Resiliency != null)
                {
                    httpClientBuilder.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(config.Resiliency.Retries));
                }
            };

            configureHttpClient(services.AddHttpClient<ISyncHashedClient, SyncHashedClient>());

            services.AddTransient<INetworkPerspectiveCore, NetworkPerspectiveCoreFacade>();

            return services;
        }
    }
}