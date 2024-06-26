﻿using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    public interface IMicrosoftAuthService
    {
        Task<AuthStartProcessResult> StartAuthProcessAsync(AuthProcess authProcess, CancellationToken stoppingToken = default);
        Task HandleCallbackAsync(Guid tenant, string state, CancellationToken stoppingToken = default);
    }

    internal class MicrosoftAuthService : IMicrosoftAuthService
    {
        private const int AuthorizationStateExpirationTimeInMinutes = 10;

        private readonly IAuthStateKeyFactory _stateKeyFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly IMemoryCache _cache;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly INetworkService _networkService;
        private readonly ILogger<MicrosoftAuthService> _logger;

        public MicrosoftAuthService(
            IAuthStateKeyFactory stateKeyFactory,
            ISecretRepositoryFactory secretRepositoryFactory,
            IMemoryCache cache,
            IStatusLoggerFactory statusLoggerFactory,
            INetworkService networkService,
            ILogger<MicrosoftAuthService> logger)
        {
            _stateKeyFactory = stateKeyFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
            _cache = cache;
            _statusLoggerFactory = statusLoggerFactory;
            _networkService = networkService;
            _logger = logger;
        }

        public async Task<AuthStartProcessResult> StartAuthProcessAsync(AuthProcess authProcess, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Starting microsoft admin consent process...");

            await _statusLoggerFactory
                .CreateForNetwork(authProcess.NetworkId)
                .LogInfoAsync("Admin consent process started", stoppingToken);

            var stateKey = _stateKeyFactory.Create();
            _cache.Set(stateKey, authProcess, DateTimeOffset.UtcNow.AddMinutes(AuthorizationStateExpirationTimeInMinutes));

            var clientId = await GetClientIdAsync(authProcess.NetworkId, stoppingToken);
            var authUri = BuildMicrosoftAuthUri(clientId, stateKey, authProcess.CallbackUri);

            _logger.LogInformation("Micorosoft admin consent process started. Unique state id: '{state}'", stateKey);

            return new AuthStartProcessResult(authUri);
        }

        public async Task HandleCallbackAsync(Guid tenant, string state, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Received admin consent callback.");

            if (!_cache.TryGetValue(state, out AuthProcess authProcess))
                throw new OAuthException("State does not match initialized value");

            var secretRepository = await _secretRepositoryFactory.CreateAsync(authProcess.NetworkId, stoppingToken);
            var tenantIdKey = string.Format(MicrosoftKeys.MicrosoftTenantIdPattern, authProcess.NetworkId);
            await secretRepository.SetSecretAsync(tenantIdKey, tenant.ToString().ToSecureString(), stoppingToken);
        }

        private async Task<SecureString> GetClientIdAsync(Guid networkId, CancellationToken stoppingToken)
        {
            var network = await _networkService.GetAsync<MicrosoftNetworkProperties>(networkId, stoppingToken);

            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);

            if (network.Properties.SyncMsTeams == true)
            {
                _logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Teams Microsoft Enterprise Application for authorization",
                    nameof(MicrosoftNetworkProperties.SyncMsTeams), network.Properties.SyncMsTeams);
                return await secretRepository.GetSecretAsync(MicrosoftKeys.MicrosoftClientTeamsIdKey, stoppingToken);
            }
            else
            {
                _logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Basic Microsoft Enterprise Application for authorization",
                    nameof(MicrosoftNetworkProperties.SyncMsTeams), network.Properties.SyncMsTeams);
                return await secretRepository.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicIdKey, stoppingToken);
            }
        }

        private string BuildMicrosoftAuthUri(SecureString microsoftClientId, string state, Uri callbackUrl)
        {
            _logger.LogDebug("Building microsoft admin consent path...");

            var uriBuilder = new UriBuilder("https://login.microsoftonline.com/common/adminconsent");

            uriBuilder.Query = string.Format("client_id={0}&state={1}&redirect_uri={2}", microsoftClientId.ToSystemString(), state, callbackUrl.ToString());

            _logger.LogDebug("Built microsoft admin consent path: '{uriBuilder}'", uriBuilder);

            return uriBuilder.ToString();
        }
    }
}