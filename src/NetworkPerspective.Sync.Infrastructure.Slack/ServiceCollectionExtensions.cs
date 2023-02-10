using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

using Polly;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlack(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            var slackBaseUrl = configurationSection.GetValue<string>("BaseUrl");
            services.Configure<AuthConfig>(configurationSection.GetSection("Auth"));

            services
                .AddHttpClient(Consts.SlackApiHttpClientName, x =>
                {
                    x.BaseAddress = new Uri(slackBaseUrl);
                })
                .AddPolicyHandler(GetRetryAfterDelayOnThrottlingPolicy());

            services.AddTransient<ISlackHttpClientFactory, SlackHttpClientFactory>();
            services.AddSingleton<ISlackAuthService, SlackAuthService>();
            services.AddSingleton<IStateKeyFactory, StateKeyFactory>();
            services.AddTransient<CursorPaginationHandler>();
            services.AddTransient<ISlackClientFacadeFactory, SlackClientFacadeFactory>();
            services.AddMemoryCache();

            services.AddSingleton<IDataSourceFactory, SlackFacadeFactory>();

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
}