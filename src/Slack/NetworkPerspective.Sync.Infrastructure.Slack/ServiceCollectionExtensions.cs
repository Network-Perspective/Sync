using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;
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
            services.Configure<Resiliency>(configurationSection.GetSection("Resiliency"));

            services
                .AddHttpClient(Consts.SlackApiHttpClientName, x =>
                {
                    x.BaseAddress = new Uri(slackBaseUrl);
                })
                .AddPolicyHandler(GetRetryAfterDelayOnThrottlingPolicy());

            services
                .AddHttpClient(Consts.SlackApiHttpClientWithBotTokenName, x =>
                {
                    x.BaseAddress = new Uri(slackBaseUrl);
                })
                .AddPolicyHandler(GetRetryAfterDelayOnThrottlingPolicy())
                .AddScopeAwareHttpHandler(sp =>
                {
                    var networkIdProvider = sp.GetRequiredService<INetworkIdProvider>();
                    var cachedSecretRepository = sp.GetRequiredService<ICachedSecretRepository>();
                    return new AuthTokenHandler(networkIdProvider, cachedSecretRepository, SlackKeys.TokenKeyPattern);
                });

            services
                .AddHttpClient(Consts.SlackApiHttpClientWithUserTokenName, x =>
                {
                    x.BaseAddress = new Uri(slackBaseUrl);
                })
                .AddPolicyHandler(GetRetryAfterDelayOnThrottlingPolicy())
                .AddScopeAwareHttpHandler(sp =>
                {
                    var networkIdProvider = sp.GetRequiredService<INetworkIdProvider>();
                    var cachedSecretRepository = sp.GetRequiredService<ICachedSecretRepository>();
                    return new AuthTokenHandler(networkIdProvider, cachedSecretRepository, SlackKeys.UserTokenKeyPattern);
                });

            services.AddTransient<IAuthTester, AuthTester>();
            services.AddScoped<ISlackAuthService, SlackAuthService>();
            services.AddTransient<CursorPaginationHandler>();
            services.AddScoped<ISlackClientFacadeFactory, SlackClientFacadeFactory>();
            services.AddScoped<ISlackClientUnauthorizedFacade>(sp => sp.GetRequiredService<ISlackClientFacadeFactory>().CreateUnauthorized());
            services.AddMemoryCache();

            services.AddScoped<IMembersClient, MembersClient>();
            services.AddScoped<IChatClient, ChatClient>();

            services.AddScoped<IDataSource, SlackFacade>();
            services.AddTransient<ISecretRotator, SlackSecretsRotator>();

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