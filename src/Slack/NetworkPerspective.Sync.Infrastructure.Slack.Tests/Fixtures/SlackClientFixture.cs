﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Configs;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Fixtures
{
    public class SlackClientFixture : IDisposable
    {
        public ISlackHttpClient SlackHttpClient { get; }

        public SlackClientFixture()
        {
            var secretStorageOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = "https://nptestvault.vault.azure.net/" });

            var secretRepository = new InternalAzureKeyVaultClient(TokenCredentialFactory.Create(), secretStorageOptions, NullLogger<InternalAzureKeyVaultClient>.Instance);
            var slackToken = secretRepository.GetSecretAsync(string.Format(SlackKeys.TokenKeyPattern, "unit-test")).Result;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://slack.com/api/")
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new NetworkCredential(string.Empty, slackToken).Password);

            var resiliency = new Resiliency
            {
                Retries = new[]
                {
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(300),
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromMilliseconds(1000)
                }
            };
            SlackHttpClient = new ResilientSlackHttpClientDecorator(
                new SlackHttpClient(httpClient, NullLogger<SlackHttpClient>.Instance),
                resiliency,
                NullLogger<ResilientSlackHttpClientDecorator>.Instance);
        }

        public void Dispose()
            => SlackHttpClient?.Dispose();
    }
}