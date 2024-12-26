using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Configs;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services;

internal class OAuthService(IVault vault, IJiraUnauthorizedFacade jiraUnauthorizedFacade, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, IOptions<JiraConfig> config, ILogger<OAuthService> logger) : IOAuthService
{
    private const int AuthorizationCodeExpirationTimeInMinutes = 10;

    public async Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Starting jira autentication process...");

        var clientId = await vault.GetSecretAsync(JiraClientKeys.JiraClientIdKey, stoppingToken);

        var stateKey = stateKeyFactory.Create();
        var stateExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(AuthorizationCodeExpirationTimeInMinutes);
        cache.Set(stateKey, context, stateExpirationTimestamp);

        var authUri = BuildAuthUri(stateKey, context, clientId);

        logger.LogInformation("Jira authentication process started. Unique state id: '{state}'", stateKey);

        return new InitializeOAuthResult(authUri, stateKey, stateExpirationTimestamp.UtcDateTime);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string code, OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received Authentication callback.");

        var clientId = await vault.GetSecretAsync(JiraClientKeys.JiraClientIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(JiraClientKeys.JiraClientSecretKey, stoppingToken);

        var tokenResponse = await jiraUnauthorizedFacade.ExchangeCodeForTokenAsync(code, clientId.ToSystemString(), clientSecret.ToSystemString(), context.CallbackUri, stoppingToken);

        var accessTokenKey = string.Format(JiraKeys.JiraAccessTokenKeyPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(accessTokenKey, tokenResponse.AccessToken.ToSecureString(), stoppingToken);

        var refreshTokenKey = string.Format(JiraKeys.JiraRefreshTokenPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(refreshTokenKey, tokenResponse.RefreshToken.ToSecureString(), stoppingToken);
    }

    private string BuildAuthUri(string state, OAuthContext context, SecureString clientId)
    {
        logger.LogDebug("Building jira auth path...'");

        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["audience"] = "api.atlessian.com";
        queryParameters["client_id"] = clientId.ToSystemString();
        queryParameters["scope"] = string.Join(' ', config.Value.Auth.Scopes);
        queryParameters["redirect_uri"] = context.CallbackUri.ToString();
        queryParameters["state"] = state;
        queryParameters["response_type"] = "code";
        queryParameters["prompt"] = "consent";

        var uriBuilder = new UriBuilder(config.Value.Auth.BaseUrl)
        {
            Path = config.Value.Auth.Path,
            Query = queryParameters.ToString()
        };

        logger.LogDebug("Built jira auth path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}