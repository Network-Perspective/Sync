﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Kiota.Authentication.Azure;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Configs;

using Polly;
using Polly.Extensions.Http;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IMicrosoftClientFactory
    {
        Task<GraphServiceClient> GetMicrosoftClientAsync(Guid networkId, CancellationToken stoppingToken = default);
    }

    internal class MicrosoftClientFactory : IMicrosoftClientFactory
    {
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly INetworkService _networkService;
        private readonly PolicyHttpMessageHandler _retryHandler;

        public MicrosoftClientFactory(ISecretRepositoryFactory secretRepositoryFactory, INetworkService networkService, IOptions<Resiliency> resiliencyOptions, ILoggerFactory loggerFactory)
        {
            _secretRepositoryFactory = secretRepositoryFactory;
            _networkService = networkService;
            var retryLogger = loggerFactory.CreateLogger<GraphServiceClient>();

            void OnRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timespan)
            {
                retryLogger.LogInformation(result.Exception, "Problem occured on calling graph api. Next attempt in {timespan}", timespan);
            };

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(resiliencyOptions.Value.Retries, OnRetry);

            _retryHandler = new PolicyHttpMessageHandler(policy);
        }

        public async Task<GraphServiceClient> GetMicrosoftClientAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var authProvider = await BuildAuthProvider(networkId, stoppingToken);
            var httpClient = BuildHttpClient();

            return new GraphServiceClient(httpClient, authProvider);
        }

        private async Task<AzureIdentityAuthenticationProvider> BuildAuthProvider(Guid networkId, CancellationToken stoppingToken)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);

            var tenantIdKey = string.Format(MicrosoftKeys.MicrosoftTenantIdPattern, networkId);
            var tenantId = await secretRepository.GetSecretAsync(tenantIdKey, stoppingToken);

            var network = await _networkService.GetAsync<MicrosoftNetworkProperties>(networkId, stoppingToken);

            var clientIdKey = network.Properties.SyncMsTeams == true
                ? MicrosoftKeys.MicrosoftClientTeamsIdKey
                : MicrosoftKeys.MicrosoftClientBasicIdKey;

            var clientId = await secretRepository.GetSecretAsync(clientIdKey, stoppingToken);

            var clientSecretKey = network.Properties.SyncMsTeams == true
                ? MicrosoftKeys.MicrosoftClientTeamsSecretKey
                : MicrosoftKeys.MicrosoftClientBasicSecretKey;

            var clientSecret = await secretRepository.GetSecretAsync(clientSecretKey, stoppingToken);

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId.ToSystemString(), clientId.ToSystemString(), clientSecret.ToSystemString(), options);
            return new AzureIdentityAuthenticationProvider(clientSecretCredential, Array.Empty<string>());
        }

        private HttpClient BuildHttpClient()
        {
            var handlers = GraphClientFactory.CreateDefaultHandlers();
            handlers.Add(_retryHandler);
            return GraphClientFactory.Create(handlers);
        }
    }
}