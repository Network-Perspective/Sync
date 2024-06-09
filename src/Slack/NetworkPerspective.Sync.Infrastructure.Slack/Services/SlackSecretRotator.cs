using System;
using System.Security;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class SlackSecretsRotator : ISecretRotator
    {
        private readonly ILogger<SlackSecretsRotator> _logger;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
        private readonly IUnitOfWork _unitOfWork;

        public SlackSecretsRotator(
            ILogger<SlackSecretsRotator> logger,
            ISecretRepositoryFactory secretRepositoryFactory,
            ISlackClientFacadeFactory slackClientFacadeFactory,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _secretRepositoryFactory = secretRepositoryFactory;
            _slackClientFacadeFactory = slackClientFacadeFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task RotateSecrets()
        {
            _logger.LogInformation("Rotating Slack secrets");
            try
            {
                var secretRepository = _secretRepositoryFactory.Create();

                var clientId = await secretRepository.GetSecretAsync(SlackKeys.SlackClientIdKey);
                var clientSecret = await secretRepository.GetSecretAsync(SlackKeys.SlackClientSecretKey);

                // for each network
                var connectors = await _unitOfWork
                    .GetConnectorRepository<SlackConnectorProperties>()
                    .GetAllAsync();

                foreach (var connector in connectors)
                {
                    _logger.LogInformation("Rotating token for network {networkId}", connector.Id);
                    try
                    {
                        await RotateNetworkSlackBotToken(connector, clientId, clientSecret);
                        _logger.LogInformation("Token rotated for network {networkId}", connector.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to rotate token for network {networkId}", connector.Id);
                    }
                }

                _logger.LogInformation("Finished rotating slack secrets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate Slack secrets");
                throw;
            }
        }

        private async Task RotateNetworkSlackBotToken(Connector<SlackConnectorProperties> connector, SecureString clientId, SecureString clientSecret)
        {
            var secretRepository = _secretRepositoryFactory.Create(connector.Properties.ExternalKeyVaultUri);
            var slackClient = _slackClientFacadeFactory.CreateUnauthorized();

            var accessTokenKey = string.Format(SlackKeys.TokenKeyPattern, connector.Id);
            var refreshTokenKey = string.Format(SlackKeys.RefreshTokenPattern, connector.Id);

            // try to get access token for the network
            // the access token might be not yet there if the network is not yet authorized
            SecureString accessToken;
            try
            {
                accessToken = await secretRepository.GetSecretAsync(accessTokenKey);
            }
            catch
            {
                _logger.LogInformation("Network {networkId} not yet authorized. No token to rotate.", connector.Id);
                return;
            }

            // try to get refresh token
            SecureString refreshToken;
            try
            {
                refreshToken = await secretRepository.GetSecretAsync(refreshTokenKey);
            }
            catch (Exception)
            {
                // Exchange token flow:
                // we got access_token but not refresh token - looks like access token wasn't yet exchanged
                // this happens when we just enabled token rotation and we have non-expiring token
                // We need to exchange it for expiring access token and refresh token
                var exchangeTokenRequest = new OAuthExchangeRequest
                {
                    ClientId = clientId.ToSystemString(),
                    ClientSecret = clientSecret.ToSystemString(),
                    AccessToken = accessToken.ToSystemString(),
                };

                var exchangeResult = await slackClient.ExchangeLegacyTokenAsync(exchangeTokenRequest);
                if (!exchangeResult.IsOk)
                {
                    _logger.LogError("Failed to exchange Slack token: {error}", exchangeResult.Error);
                    return;
                }

                await secretRepository.SetSecretAsync(refreshTokenKey, exchangeResult.RefreshToken.ToSecureString());
                await secretRepository.SetSecretAsync(accessTokenKey, exchangeResult.AccessToken.ToSecureString());

                // no need to refresh token if we just exchanged it
                return;
            }

            // Refresh token flow:
            // we have access token and refresh token - we need to refresh access token
            // using the refresh token. Better to do it before tokens expire.
            var request = new OAuthAccessRequest
            {
                ClientId = clientId.ToSystemString(),
                ClientSecret = clientSecret.ToSystemString(),
                GrantType = "refresh_token",
                RefreshToken = refreshToken.ToSystemString()
            };

            var refreshResult = await slackClient.AccessAsync(request);

            if (!refreshResult.IsOk)
            {
                _logger.LogError("Failed to exchange Slack token: {error}", refreshResult.Error);
                return;
            }
            await secretRepository.SetSecretAsync(refreshTokenKey, refreshResult.RefreshToken.ToSecureString());
            await secretRepository.SetSecretAsync(accessTokenKey, refreshResult.AccessToken.ToSecureString());
        }
    }
}