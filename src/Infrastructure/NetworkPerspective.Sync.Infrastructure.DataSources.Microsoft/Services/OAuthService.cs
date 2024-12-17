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


namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal class OAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, ILogger<OAuthService> logger) : IOAuthService
{
    private const int AuthorizationStateExpirationTimeInMinutes = 10;

    private const string MicrosoftTenantIdPattern = "microsoft-tenant-id-{0}";
    public const string MicrosoftClientBasicIdKey = "microsoft-client-basic-id";
    private const string MicrosoftClientTeamsIdKey = "microsoft-client-with-teams-id";

    public async Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Starting microsoft admin consent process...");

        var stateKey = stateKeyFactory.Create();
        var stateExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(AuthorizationStateExpirationTimeInMinutes);
        cache.Set(stateKey, context, stateExpirationTimestamp);

        var clientId = await GetClientIdAsync(context.Connector.GetConnectorProperties<MicrosoftNetworkProperties>().SyncMsTeams, stoppingToken);
        var authUri = BuildMicrosoftAuthUri(clientId, stateKey, context.CallbackUri);

        logger.LogInformation("Micorosoft admin consent process started. Unique state id: '{state}'", stateKey);

        return new InitializeOAuthResult(authUri, stateKey, stateExpirationTimestamp.UtcDateTime);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string tenantId, OAuthContext context, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received admin consent callback.");

        var tenantIdKey = string.Format(MicrosoftTenantIdPattern, context.Connector.ConnectorId);
        await vault.SetSecretAsync(tenantIdKey, tenantId.ToSecureString(), stoppingToken);
    }

    private async Task<SecureString> GetClientIdAsync(bool syncMsTeams, CancellationToken stoppingToken)
    {
        if (syncMsTeams == true)
        {
            logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Teams Microsoft Enterprise Application for authorization",
                nameof(syncMsTeams), syncMsTeams);
            return await vault.GetSecretAsync(MicrosoftClientTeamsIdKey, stoppingToken);
        }
        else
        {
            logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Basic Microsoft Enterprise Application for authorization",
                nameof(syncMsTeams), syncMsTeams);
            return await vault.GetSecretAsync(MicrosoftClientBasicIdKey, stoppingToken);
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