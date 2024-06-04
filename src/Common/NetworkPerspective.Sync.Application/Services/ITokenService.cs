using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ITokenService
    {
        Task AddOrReplace(SecureString accessToken, Guid connectorId, CancellationToken stoppingToken = default);
        Task<SecureString> GetAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task<bool> HasValidAsync(Guid connectorId, CancellationToken stoppingToken = default);
    }

    internal class TokenService : ITokenService
    {
        private readonly MiscConfig _config;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly ILogger<TokenService> _logger;

        public TokenService(ISecretRepositoryFactory secretRepositoryFactory, INetworkPerspectiveCore networkPerspectiveCore, IOptions<MiscConfig> config, ILogger<TokenService> logger)
        {
            _config = config.Value;
            _secretRepositoryFactory = secretRepositoryFactory;
            _networkPerspectiveCore = networkPerspectiveCore;
            _logger = logger;
        }

        public async Task AddOrReplace(SecureString accessToken, Guid connectorId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Saving Access Token for connector '{connectorId}'...", connectorId);

            var tokenKey = GetAccessTokenKey(connectorId);
            var secretRepository = await _secretRepositoryFactory.CreateAsync(connectorId, stoppingToken);
            await secretRepository.SetSecretAsync(tokenKey, accessToken, stoppingToken);

            _logger.LogDebug("Access Token for connector '{connectorId}' saved", connectorId);
        }

        public async Task<SecureString> GetAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var tokenKey = GetAccessTokenKey(connectorId);
            var secretRepository = await _secretRepositoryFactory.CreateAsync(connectorId, stoppingToken);
            return await secretRepository.GetSecretAsync(tokenKey, stoppingToken);
        }

        public async Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Removing Access Token for connector '{connectorId}'...", connectorId);
                var tokenKey = GetAccessTokenKey(connectorId);
                var secretRepository = await _secretRepositoryFactory.CreateAsync(connectorId, stoppingToken);
                await secretRepository.RemoveSecretAsync(tokenKey, stoppingToken);
                _logger.LogDebug("Removed Access Token for connector '{connectorId}'...", connectorId);
            }
            catch (Exception)
            {
                _logger.LogDebug("Unable to remove Access Token for connector '{connectorId}', maybe there is nothing to remove?", connectorId);
            }
        }

        public async Task<bool> HasValidAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var accessToken = await GetAsync(connectorId, stoppingToken);

            try
            {
                var authResult = await _networkPerspectiveCore.ValidateTokenAsync(accessToken, stoppingToken);
                return authResult.Id == connectorId;
            }
            catch (InvalidTokenException)
            {
                return false;
            }
        }

        private string GetAccessTokenKey(Guid connectorId)
            => string.Format(Keys.TokenKeyPattern, _config.DataSourceName, connectorId.ToString());
    }
}