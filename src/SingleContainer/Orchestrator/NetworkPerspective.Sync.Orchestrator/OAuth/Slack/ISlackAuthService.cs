using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Slack;

public interface ISlackAuthService
{
    Task<SlackAuthStartProcessResult> StartAuthProcessAsync(SlackAuthProcess authProcess, CancellationToken stoppingToken = default);
    Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
}

internal class SlackAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, IWorkerRouter workerRouter, IOptions<SlackAuthConfig> slackAuthConfig, ISlackClientUnauthorizedFacade slackClientUnauthorizedFacade, ILogger<SlackAuthService> logger) : ISlackAuthService
{
    private const int SlackAuthorizationCodeExpirationTimeInMinutes = 10;
    public const string SlackClientIdKey = "slack-client-id";
    private const string SlackClientSecretKey = "slack-client-secret";
    private const string SlackBotTokenKeyPattern = "slack-bot-token-{0}";
    private const string SlackUserTokenKeyPattern = "slack-user-token-{0}";
    private const string SlackRefreshTokenPattern = "slack-refresh-token-{0}";

    public async Task<SlackAuthStartProcessResult> StartAuthProcessAsync(SlackAuthProcess authProcess, CancellationToken stoppingToken = default)
    {

        logger.LogInformation("Starting slack autentication process...");

        var clientId = await vault.GetSecretAsync(SlackClientIdKey, stoppingToken);

        var stateKey = stateKeyFactory.Create();
        cache.Set(stateKey, authProcess, DateTimeOffset.UtcNow.AddMinutes(SlackAuthorizationCodeExpirationTimeInMinutes));

        var authUri = BuildSlackAuthUri(stateKey, authProcess, clientId);

        logger.LogInformation("Slack authentication process started. Unique state id: '{state}'", stateKey);

        return new SlackAuthStartProcessResult(authUri);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received Authentication callback.");

        if (!cache.TryGetValue(state, out SlackAuthProcess authProcess))
            throw new OAuthException("State does not match initialized value");

        var clientId = await vault.GetSecretAsync(SlackClientIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(SlackClientSecretKey, stoppingToken);

        var request = new OAuthAccessRequest
        {
            ClientId = clientId.ToSystemString(),
            ClientSecret = clientSecret.ToSystemString(),
            GrantType = "authorization_code",
            RedirectUri = authProcess.CallbackUri.ToString(),
            Code = code,
        };

        var response = await slackClientUnauthorizedFacade.AccessAsync(request, stoppingToken);

        var secrets = new Dictionary<string, SecureString>();

        var botTokenKey = string.Format(SlackBotTokenKeyPattern, authProcess.ConnectorId);
        secrets.Add(botTokenKey, response.AccessToken.ToSecureString());

        // save refresh token if token rotation is enabled
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            var refreshTokenKey = string.Format(SlackRefreshTokenPattern, authProcess.ConnectorId);
            secrets.Add(refreshTokenKey, response.RefreshToken.ToSecureString());
        }

        if (authProcess.RequireAdminPrivileges)
        {
            var userTokenKey = string.Format(SlackUserTokenKeyPattern, authProcess.ConnectorId);
            secrets.Add(userTokenKey, response.User.AccessToken.ToSecureString());
        }

        await workerRouter.SetSecretsAsync(authProcess.WorkerName, secrets);
        logger.LogInformation("Authentication callback processed successfully. Connector '{connectorId}' is configured for synchronization", authProcess.ConnectorId);
    }

    private string BuildSlackAuthUri(string state, SlackAuthProcess authProcess, SecureString slackClientId)
    {
        logger.LogDebug("Building slack auth path... The {parameter} is set to '{value}'", nameof(authProcess.RequireAdminPrivileges), authProcess.RequireAdminPrivileges);

        var uriBuilder = new UriBuilder("https://slack.com/oauth/v2/authorize");

        var scopesParameter = string.Join(',', slackAuthConfig.Value.Scopes);

        var userScopes = authProcess.RequireAdminPrivileges
            ? slackAuthConfig.Value.UserScopes.Union(slackAuthConfig.Value.AdminUserScopes)
            : slackAuthConfig.Value.UserScopes;
        var userScopesParameter = string.Join(',', userScopes);

        uriBuilder.Query = string.Format("redirect_uri={0}&client_id={1}&scope={2}&user_scope={3}&state={4}", authProcess.CallbackUri.ToString(), slackClientId.ToSystemString(), scopesParameter, userScopesParameter, state);

        logger.LogDebug("Built slack auth path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}