using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class SlackSecretsRotator : ISecretRotator
    {
        private readonly ILogger<SlackSecretsRotator> _logger;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly ISlackHttpClientFactory _slackHttpClientFactory;
        private readonly IUnitOfWork _unitOfWork;

        public SlackSecretsRotator(
            ILogger<SlackSecretsRotator> logger,
            ISecretRepositoryFactory secretRepositoryFactory,
            ISlackHttpClientFactory slackHttpClientFactory,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _secretRepositoryFactory = secretRepositoryFactory;
            _slackHttpClientFactory = slackHttpClientFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task RotateSecrets()
        {
            _logger.LogInformation("Rotating Slack secrets");
            try
            {
                var secretRepository = _secretRepositoryFactory.CreateDefault();

                var clientId = await secretRepository.GetSecretAsync(SlackKeys.SlackClientIdKey);
                var clientSecret = await secretRepository.GetSecretAsync(SlackKeys.SlackClientSecretKey);

                // for each network
                var networks = await _unitOfWork.GetNetworkRepository<SlackNetworkProperties>().GetAllAsync();
                foreach (var network in networks)
                {
                    _logger.LogInformation("Rotating token for network {networkId}", network.NetworkId);
                    try
                    {
                        await RotateNetworkSlackBotToken(network.NetworkId, clientId, clientSecret);
                        _logger.LogInformation("Token rotated for network {networkId}", network.NetworkId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to rotate token for network {networkId}", network.NetworkId);
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

        private async Task RotateNetworkSlackBotToken(Guid networkId, SecureString clientId, SecureString clientSecret)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId);
            var slackClient = _slackHttpClientFactory.Create();

            var accessTokenKey = string.Format(SlackKeys.TokenKeyPattern, networkId);
            var refreshTokenKey = string.Format(SlackKeys.RefreshTokenPattern, networkId);

            // try to get access token for the network
            // the access token might be not yet there if the network is not yet authorized
            SecureString accessToken;
            try
            {
                accessToken = await secretRepository.GetSecretAsync(accessTokenKey);
            }
            catch
            {
                _logger.LogInformation("Network {networkId} not yet authorized. No token to rotate.", networkId);
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
                var exchangeTokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId.ToSystemString()),
                    new KeyValuePair<string, string>("client_secret", clientSecret.ToSystemString()),
                    new KeyValuePair<string, string>("token", accessToken.ToSystemString())
                });
                var exchangeResult = await slackClient.PostAsync<TokenExchangeResponse>("oauth.v2.exchange", exchangeTokenRequest);
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
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId.ToSystemString()),
                new KeyValuePair<string, string>("client_secret", clientSecret.ToSystemString()),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken.ToSystemString())
            });

            var refreshResult = await slackClient.PostAsync<TokenExchangeResponse>("oauth.v2.access", requestContent);

            if (!refreshResult.IsOk)
            {
                _logger.LogError("Failed to exchange Slack token: {error}", refreshResult.Error);
                return;
            }
            await secretRepository.SetSecretAsync(refreshTokenKey, refreshResult.RefreshToken.ToSecureString());
            await secretRepository.SetSecretAsync(accessTokenKey, refreshResult.AccessToken.ToSecureString());
        }
    }

    internal class TokenExchangeResponse : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}