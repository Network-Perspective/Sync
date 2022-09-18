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
        Task AddOrReplace(SecureString accessToken, Guid networkId, CancellationToken stoppingToken = default);
        Task<SecureString> GetAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task EnsureRemovedAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task<bool> HasValidAsync(Guid networkId, CancellationToken stoppingToken = default);
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

        public async Task AddOrReplace(SecureString accessToken, Guid networkId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Saving Access Token for network '{networkId}'...", networkId);

            var tokenKey = GetAccessTokenKey(networkId);
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
            await secretRepository.SetSecretAsync(tokenKey, accessToken, stoppingToken);

            _logger.LogDebug("Access Token for network '{networkId}' saved", networkId);
        }

        public async Task<SecureString> GetAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var tokenKey = GetAccessTokenKey(networkId);
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
            return await secretRepository.GetSecretAsync(tokenKey, stoppingToken);
        }

        public async Task EnsureRemovedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Removing Access Token for network '{networkId}'...", networkId);
                var tokenKey = GetAccessTokenKey(networkId);
                var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
                await secretRepository.RemoveSecretAsync(tokenKey, stoppingToken);
                _logger.LogDebug("Removed Access Token for network '{networkId}'...", networkId);
            }
            catch (Exception)
            {
                _logger.LogDebug("Unable to remove Access Token for network '{networkId}', maybe there is nothing to remove?", networkId);
            }
        }

        public async Task<bool> HasValidAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var accessToken = await GetAsync(networkId, stoppingToken);

            try
            {
                var authResult = await _networkPerspectiveCore.ValidateTokenAsync(accessToken, stoppingToken);
                return authResult.NetworkId == networkId;
            }
            catch (InvalidTokenException)
            {
                return false;
            }
        }

        private string GetAccessTokenKey(Guid networkId)
            => string.Format(Keys.TokenKeyPattern, _config.DataSourceName, networkId.ToString());
    }
}