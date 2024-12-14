using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class SlackSecretRoationService(IVault secretRepository, ISlackClientFacadeFactory slackClientFacadeFactory, IConnectorContextAccessor connectorInfoProvider, ILogger<SlackSecretRoationService> logger) : ISecretRotationService
{
    public async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        var connectorContext = connectorInfoProvider.Context;

        logger.LogInformation("Rotating token for connector {connectorId}", connectorContext.ConnectorId);

        var connectorProperties = connectorContext.GetConnectorProperties();

        var slackClient = slackClientFacadeFactory.CreateUnauthorized();

        var accessTokenKey = string.Format(SlackKeys.BotTokenKeyPattern, connectorContext.ConnectorId);
        var refreshTokenKey = string.Format(SlackKeys.RefreshTokenPattern, connectorContext.ConnectorId);

        var clientId = await secretRepository.GetSecretAsync(SlackKeys.SlackClientIdKey, stoppingToken);
        var clientSecret = await secretRepository.GetSecretAsync(SlackKeys.SlackClientSecretKey, stoppingToken);

        // try to get access token for the network
        // the access token might be not yet there if the network is not yet authorized
        SecureString accessToken;
        try
        {
            accessToken = await secretRepository.GetSecretAsync(accessTokenKey, stoppingToken);
        }
        catch
        {
            logger.LogInformation("Network {networkId} not yet authorized. No token to rotate.", connectorContext.ConnectorId);
            return;
        }

        // try to get refresh token
        SecureString refreshToken;
        try
        {
            refreshToken = await secretRepository.GetSecretAsync(refreshTokenKey, stoppingToken);
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
                logger.LogError("Failed to exchange Slack token: {error}", exchangeResult.Error);
                return;
            }

            await secretRepository.SetSecretAsync(refreshTokenKey, exchangeResult.RefreshToken.ToSecureString(), stoppingToken);
            await secretRepository.SetSecretAsync(accessTokenKey, exchangeResult.AccessToken.ToSecureString(), stoppingToken);

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
            logger.LogError("Failed to exchange Slack token: {error}", refreshResult.Error);
            return;
        }
        await secretRepository.SetSecretAsync(refreshTokenKey, refreshResult.RefreshToken.ToSecureString(), stoppingToken);
        await secretRepository.SetSecretAsync(accessTokenKey, refreshResult.AccessToken.ToSecureString(), stoppingToken);

        logger.LogInformation("Token rotated for connector {connectorId}", connectorContext.ConnectorId);
    }
}