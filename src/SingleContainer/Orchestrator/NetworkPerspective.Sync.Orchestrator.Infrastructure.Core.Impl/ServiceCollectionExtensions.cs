using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Infrastructure.Core.HealthChecks;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;

using Polly;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Impl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfigurationSection configurationSection, IHealthChecksBuilder healthChecksBuilder)
    {
        var config = new CoreConfig();
        configurationSection.Bind(config);
        services.Configure<CoreConfig>(configurationSection);

        healthChecksBuilder
            .AddCheck<CoreHealthCheck>("Network-Perspective-Core", HealthStatus.Unhealthy, Array.Empty<string>());


        var httpClientBuilder = services
            .AddHttpClient<ISyncHashedClient, SyncHashedClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(config.BaseUrl));

        if (config.Resiliency is not null)
        {
            httpClientBuilder
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(config.Resiliency.Retries));
        };

        services.AddHttpClient<ISettingsClient, SettingsClient>();

        services.AddTransient<ICore, Core>();


        return services;
    }
}