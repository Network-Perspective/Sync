﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackHttpClientFactory
    {
        Task<ISlackHttpClient> CreateAsync(Guid networkId, CancellationToken stoppingToken = default);
        ISlackHttpClient CreateUnauthorized();
    }

    internal class SlackHttpClientFactory : ISlackHttpClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;

        public SlackHttpClientFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, ISecretRepositoryFactory secretRepositoryFactory)
        {
            _loggerFactory = loggerFactory;
            _httpClientFactory = httpClientFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
        }

        public async Task<ISlackHttpClient> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
            var tokenKey = string.Format(SlackKeys.TokenKeyPattern, networkId);
            var accessToken = await secretRepository.GetSecretAsync(tokenKey, stoppingToken);

            var httpClient = _httpClientFactory.CreateClient(Consts.SlackApiHttpClientName);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.ToSystemString());
            var slackHttpClient = new SlackHttpClient(httpClient, _loggerFactory.CreateLogger<SlackHttpClient>());
            return new ResilientSlackHttpClientDecorator(slackHttpClient, _loggerFactory.CreateLogger<ResilientSlackHttpClientDecorator>());
        }

        public ISlackHttpClient CreateUnauthorized()
        {
            var httpClient = _httpClientFactory.CreateClient(Consts.SlackApiHttpClientName);

            var slackHttpClient = new SlackHttpClient(httpClient, _loggerFactory.CreateLogger<SlackHttpClient>());
            return new ResilientSlackHttpClientDecorator(slackHttpClient, _loggerFactory.CreateLogger<ResilientSlackHttpClientDecorator>());
        }
    }
}