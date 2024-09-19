using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;

using Polly;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Slack;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSlackAuth(this IServiceCollection services, IConfigurationSection configuration)
    {
        var slackBaseUrl = configuration.GetValue<string>("BaseUrl");
        var resiliencyConfigurationSection = configuration.GetSection("Resiliency");
        var authConfigSection = configuration.GetSection("Auth");

        services.Configure<SlackAuthConfig>(authConfigSection);

        services
            .AddHttpClient(Consts.SlackApiHttpClientName, x =>
            {
                x.BaseAddress = new Uri(slackBaseUrl);
            })
            .AddPolicyHandler(GetRetryAfterDelayOnThrottlingPolicy());


        services.AddSlackClient(resiliencyConfigurationSection);

        services.AddMemoryCache();

        services.AddScoped<ISlackAuthService, SlackAuthService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryAfterDelayOnThrottlingPolicy()
    {
        static TimeSpan SleepDurationProvider(int _, DelegateResult<HttpResponseMessage> response, Context context)
        {
            return response.Result?.Headers.RetryAfter.Delta ?? TimeSpan.FromSeconds(60);
        }

        static Task OnRetryAsync(DelegateResult<HttpResponseMessage> response, TimeSpan timespan, int retryCount, Context context)
        {
            return Task.CompletedTask;
        }

        return Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(30, SleepDurationProvider, OnRetryAsync);
    }
}