using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Clients;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal class OAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IOAuthClient authClient, IMemoryCache cache, ILogger<OAuthService> logger) : IOAuthService
{
    private const int AuthorizationCodeExpirationTimeInMinutes = 10;
    private readonly string[] _scopes = [
        DirectoryService.Scope.AdminDirectoryUserReadonly,
    ];

    public async Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Starting google autentication process...");

        var clientId = await vault.GetSecretAsync(GoogleKeys.GoogleClientIdKey, stoppingToken);

        var stateKey = stateKeyFactory.Create();
        var stateExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(AuthorizationCodeExpirationTimeInMinutes);
        cache.Set(stateKey, context, stateExpirationTimestamp);

        var authUri = BuildAuthUri(stateKey, context.CallbackUri, clientId);

        logger.LogInformation("Google authentication process started. Unique state id: '{state}'", stateKey);

        return new InitializeOAuthResult(authUri, stateKey, stateExpirationTimestamp.UtcDateTime);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string code, OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received Authentication callback.");

        var clientId = await vault.GetSecretAsync(GoogleKeys.GoogleClientIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(GoogleKeys.GoogleClientSecretKey, stoppingToken);

        var tokenResponse = await authClient.ExchangeCodeForTokenAsync(code, clientId.ToSystemString(), clientSecret.ToSystemString(), context.CallbackUri, stoppingToken);

        var accessTokenKey = string.Format(GoogleKeys.GoogleAccessTokenKeyPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(accessTokenKey, tokenResponse.AccessToken.ToSecureString(), stoppingToken);

        var refreshTokenKey = string.Format(GoogleKeys.GoogleRefreshTokenPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(refreshTokenKey, tokenResponse.RefreshToken.ToSecureString(), stoppingToken);
    }

    private string BuildAuthUri(string state, string callbackUri, SecureString clientId)
    {
        logger.LogDebug("Building google auth path...'");

        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["client_id"] = clientId.ToSystemString();
        queryParameters["scope"] = string.Join(' ', _scopes);
        queryParameters["redirect_uri"] = callbackUri.ToString();
        queryParameters["state"] = state;
        queryParameters["response_type"] = "code";
        queryParameters["access_type"] = "offline";
        queryParameters["prompt"] = "consent";

        var uriBuilder = new UriBuilder(GoogleAuthConsts.OidcAuthorizationUrl)
        {
            Query = queryParameters.ToString()
        };

        logger.LogDebug("Built google auth path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}