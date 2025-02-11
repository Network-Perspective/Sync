﻿using System;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class OAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, IOptions<AuthConfig> slackAuthConfig, ISlackClientUnauthorizedFacade slackClientUnauthorizedFacade, ILogger<OAuthService> logger) : IOAuthService
{
    private const int SlackAuthorizationCodeExpirationTimeInMinutes = 10;
    public const string SlackClientIdKey = "slack-client-id";
    private const string SlackClientSecretKey = "slack-client-secret";
    private const string SlackBotTokenKeyPattern = "slack-bot-token-{0}";
    private const string SlackUserTokenKeyPattern = "slack-user-token-{0}";
    private const string SlackRefreshTokenPattern = "slack-refresh-token-{0}";

    public async Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Initializing slack autentication process...");

        var clientId = await vault.GetSecretAsync(SlackClientIdKey, stoppingToken);

        var stateKey = stateKeyFactory.Create();
        var stateExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(SlackAuthorizationCodeExpirationTimeInMinutes);
        cache.Set(stateKey, context, stateExpirationTimestamp);

        var authUri = BuildSlackAuthUri(stateKey, context, clientId);

        logger.LogInformation("Slack authentication process started. Unique state id: '{state}'", stateKey);

        return new InitializeOAuthResult(authUri, stateKey, stateExpirationTimestamp.UtcDateTime);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string code, OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received Authentication callback");

        var clientId = await vault.GetSecretAsync(SlackClientIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(SlackClientSecretKey, stoppingToken);

        var request = new OAuthAccessRequest
        {
            ClientId = clientId.ToSystemString(),
            ClientSecret = clientSecret.ToSystemString(),
            GrantType = "authorization_code",
            RedirectUri = context.CallbackUri.ToString(),
            Code = code,
        };

        var response = await slackClientUnauthorizedFacade.AccessAsync(request, stoppingToken);

        var botTokenKey = string.Format(SlackBotTokenKeyPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(botTokenKey, response.AccessToken.ToSecureString(), stoppingToken);

        // save refresh token if token rotation is enabled
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            var refreshTokenKey = string.Format(SlackRefreshTokenPattern, context.Connector.ConnectorId);
            await vault.SetSecretAsync(refreshTokenKey, response.RefreshToken.ToSecureString(), stoppingToken);
        }

        var connectorProperties = new SlackConnectorProperties(context.Connector.Properties);

        if (connectorProperties.UsesAdminPrivileges)
        {
            var userTokenKey = string.Format(SlackUserTokenKeyPattern, context.Connector.ConnectorId);
            await vault.SetSecretAsync(userTokenKey, response.User.AccessToken.ToSecureString(), stoppingToken);
        }

        logger.LogInformation("Authentication callback processed successfully. Connector '{connectorId}' is configured for synchronization", context.Connector.ConnectorId);
    }

    private string BuildSlackAuthUri(string state, OAuthContext context, SecureString slackClientId)
    {
        var connectorProperties = new SlackConnectorProperties(context.Connector.Properties);

        logger.LogDebug("Building slack auth path... The {parameter} is set to '{value}'", nameof(connectorProperties.UsesAdminPrivileges), connectorProperties.UsesAdminPrivileges);

        var uriBuilder = new UriBuilder("https://slack.com/oauth/v2/authorize");

        var scopesParameter = string.Join(',', slackAuthConfig.Value.Scopes);

        var userScopes = connectorProperties.UsesAdminPrivileges
            ? slackAuthConfig.Value.UserScopes.Union(slackAuthConfig.Value.AdminUserScopes)
            : slackAuthConfig.Value.UserScopes;
        var userScopesParameter = string.Join(',', userScopes);

        uriBuilder.Query = string.Format("redirect_uri={0}&client_id={1}&scope={2}&user_scope={3}&state={4}", context.CallbackUri.ToString(), slackClientId.ToSystemString(), scopesParameter, userScopesParameter, state);

        logger.LogDebug("Built slack auth path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}