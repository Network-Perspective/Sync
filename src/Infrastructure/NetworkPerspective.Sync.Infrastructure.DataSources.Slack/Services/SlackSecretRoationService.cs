using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.SecretRotation;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class SlackSecretRoationService : ISecretRotationService
{
    private readonly IVault _secretRepository;
    private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
    private readonly ILogger<SlackSecretRoationService> _logger;

    public SlackSecretRoationService(IVault secretRepository, ISlackClientFacadeFactory slackClientFacadeFactory, ILogger<SlackSecretRoationService> logger)
    {
        _secretRepository = secretRepository;
        _slackClientFacadeFactory = slackClientFacadeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(SecretRotationContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Rotating token for connector {connectorId}", context.ConnectorId);

        var connectorProperties = context.GetConnectorProperties();

        var slackClient = _slackClientFacadeFactory.CreateUnauthorized();

        var accessTokenKey = string.Format(SlackKeys.TokenKeyPattern, context.ConnectorId);
        var refreshTokenKey = string.Format(SlackKeys.RefreshTokenPattern, context.ConnectorId);

        var clientId = await _secretRepository.GetSecretAsync(SlackKeys.SlackClientIdKey, stoppingToken);
        var clientSecret = await _secretRepository.GetSecretAsync(SlackKeys.SlackClientSecretKey, stoppingToken);

        // try to get access token for the network
        // the access token might be not yet there if the network is not yet authorized
        SecureString accessToken;
        try
        {
            accessToken = await _secretRepository.GetSecretAsync(accessTokenKey, stoppingToken);
        }
        catch
        {
            _logger.LogInformation("Network {networkId} not yet authorized. No token to rotate.", context.ConnectorId);
            return;
        }

        // try to get refresh token
        SecureString refreshToken;
        try
        {
            refreshToken = await _secretRepository.GetSecretAsync(refreshTokenKey, stoppingToken);
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

            await _secretRepository.SetSecretAsync(refreshTokenKey, exchangeResult.RefreshToken.ToSecureString(), stoppingToken);
            await _secretRepository.SetSecretAsync(accessTokenKey, exchangeResult.AccessToken.ToSecureString(), stoppingToken);

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
        await _secretRepository.SetSecretAsync(refreshTokenKey, refreshResult.RefreshToken.ToSecureString(), stoppingToken);
        await _secretRepository.SetSecretAsync(accessTokenKey, refreshResult.AccessToken.ToSecureString(), stoppingToken);

        _logger.LogInformation("Token rotated for connector {connectorId}", context.ConnectorId);
    }
}