﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

using Polly;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlack(this IServiceCollection services, IConfigurationSection configurationSection, ConnectorType connectorType)
        {
            services.AddSlackClient(configurationSection.GetSection("Resiliency"));

            var slackBaseUrl = configurationSection.GetValue<string>("BaseUrl");
            services.Configure<AuthConfig>(configurationSection.GetSection("Auth"));

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
                .AddScopeAwareHttpHandler<BotTokenAuthHandler>();

            services
                .AddHttpClient(Consts.SlackApiHttpClientWithUserTokenName, x =>
                {
                    x.BaseAddress = new Uri(slackBaseUrl);
                })
                .AddPolicyHandler(GetRetryAfterDelayOnThrottlingPolicy())
                .AddScopeAwareHttpHandler<UserTokenAuthHandler>();

            services.AddMemoryCache();

            services.AddTransient<ICapabilityTester>(x =>
            {
                var vault = x.GetRequiredService<IVault>();
                var logger = x.GetRequiredService<ILogger<CapabilityTester>>();
                return new CapabilityTester(connectorType, vault, logger);
            });

            services.AddScoped<IMembersClient, MembersClient>();
            services.AddScoped<IChatClient, ChatClient>();


            services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());
            services.AddKeyedScoped<IDataSource, SlackFacade>(connectorType.GetKeyOf<IDataSource>());
            services.AddKeyedScoped<IOAuthService, OAuthService>(connectorType.GetKeyOf<IOAuthService>());

            services.AddTransient<ISecretRotationService, SlackSecretRoationService>();

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