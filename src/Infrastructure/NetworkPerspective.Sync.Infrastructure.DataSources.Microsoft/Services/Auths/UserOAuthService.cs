using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;

internal class UserOAuthService(ICachedVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, IConfidentialClientAppProvider appProvider, IOptions<AuthConfig> authOptions, ILogger<UserOAuthService> logger) : IOAuthService
{
    private const int AuthorizationStateExpirationTimeInMinutes = 10;

    public async Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default)
    {
        var stateKey = stateKeyFactory.Create();
        var stateExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(AuthorizationStateExpirationTimeInMinutes);
        cache.Set(stateKey, context, stateExpirationTimestamp);

        var clientId = await vault.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicIdKey, stoppingToken);

        var authUri = BuildMicrosoftAuthUri(clientId, stateKey, context.CallbackUri);

        return new InitializeOAuthResult(authUri, stateKey, stateExpirationTimestamp.UtcDateTime);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string code, OAuthContext context, CancellationToken stoppingToken = default)
    {
        var app = await appProvider.GetWithRedirectUrlAsync(context.CallbackUri, stoppingToken);

        var result = await app
            .AcquireTokenByAuthorizationCode(authOptions.Value.Scopes, code)
            .ExecuteAsync(stoppingToken);

        var userKey = string.Format(MicrosoftKeys.UserKeyPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(userKey, result.Account.HomeAccountId.Identifier.ToSecureString(), stoppingToken);
    }

    private string BuildMicrosoftAuthUri(SecureString clientId, string state, string callbackUrl)
    {
        logger.LogDebug("Building microsoft admin consent path...");

        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["client_id"] = clientId.ToSystemString();
        queryParameters["response_type"] = "code";
        queryParameters["redirect_uri"] = callbackUrl.ToString();
        queryParameters["response_mode"] = "query";
        queryParameters["scope"] = string.Join(' ', authOptions.Value.Scopes);
        queryParameters["state"] = state;

        var uriBuilder = new UriBuilder($"https://login.microsoftonline.com/common/oauth2/v2.0/authorize")
        {
            Query = queryParameters.ToString()
        };

        logger.LogDebug("Built microsoft admin consent path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}