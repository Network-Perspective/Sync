using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;


namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;

internal class AdminConsentOAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, ILogger<AdminConsentOAuthService> logger) : IOAuthService
{
    private const int AuthorizationStateExpirationTimeInMinutes = 10;

    public async Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Starting microsoft admin consent process...");

        var stateKey = stateKeyFactory.Create();
        var stateExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(AuthorizationStateExpirationTimeInMinutes);
        cache.Set(stateKey, context, stateExpirationTimestamp);

        var connectorProperties = new MicrosoftConnectorProperties(context.Connector.Properties);
        var clientId = await GetClientIdAsync(connectorProperties.SyncMsTeams, stoppingToken);
        var authUri = BuildMicrosoftAuthUri(clientId, stateKey, context.CallbackUri);

        logger.LogInformation("Micorosoft admin consent process started. Unique state id: '{state}'", stateKey);

        return new InitializeOAuthResult(authUri, stateKey, stateExpirationTimestamp.UtcDateTime);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string tenantId, OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received admin consent callback.");

        var tenantIdKey = string.Format(MicrosoftKeys.MicrosoftTenantIdPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(tenantIdKey, tenantId.ToSecureString(), stoppingToken);
    }

    private async Task<SecureString> GetClientIdAsync(bool syncMsTeams, CancellationToken stoppingToken)
    {
        if (syncMsTeams == true)
        {
            logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Teams Microsoft Enterprise Application for authorization",
                nameof(syncMsTeams), syncMsTeams);
            return await vault.GetSecretAsync(MicrosoftKeys.MicrosoftClientTeamsIdKey, stoppingToken);
        }
        else
        {
            logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Basic Microsoft Enterprise Application for authorization",
                nameof(syncMsTeams), syncMsTeams);
            return await vault.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicIdKey, stoppingToken);
        }
    }

    private string BuildMicrosoftAuthUri(SecureString microsoftClientId, string state, string callbackUrl)
    {
        logger.LogDebug("Building microsoft admin consent path...");

        var uriBuilder = new UriBuilder("https://login.microsoftonline.com/common/adminconsent")
        {
            Query = string.Format("client_id={0}&state={1}&redirect_uri={2}", microsoftClientId.ToSystemString(), state, callbackUrl)
        };

        logger.LogDebug("Built microsoft admin consent path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}