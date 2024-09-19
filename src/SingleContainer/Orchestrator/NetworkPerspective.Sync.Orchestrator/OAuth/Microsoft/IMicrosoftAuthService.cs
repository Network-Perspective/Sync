using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Microsoft;

public interface IMicrosoftAuthService
{
    Task<MicrosoftAuthStartProcessResult> StartAuthProcessAsync(MicrosoftAuthProcess authProcess, CancellationToken stoppingToken = default);
    Task HandleCallbackAsync(Guid tenant, string state, CancellationToken stoppingToken = default);
}

internal class MicrosoftAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, ILogger<MicrosoftAuthService> logger) : IMicrosoftAuthService
{
    private const int AuthorizationStateExpirationTimeInMinutes = 10;

    private const string MicrosoftTenantIdPattern = "microsoft-tenant-id-{0}";
    public const string MicrosoftClientBasicIdKey = "microsoft-client-basic-id";
    private const string MicrosoftClientTeamsIdKey = "microsoft-client-with-teams-id";

    private readonly IVault _vault = vault;
    private readonly IAuthStateKeyFactory _stateKeyFactory = stateKeyFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<MicrosoftAuthService> _logger = logger;

    public async Task<MicrosoftAuthStartProcessResult> StartAuthProcessAsync(MicrosoftAuthProcess authProcess, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Starting microsoft admin consent process...");

        var stateKey = _stateKeyFactory.Create();
        _cache.Set(stateKey, authProcess, DateTimeOffset.UtcNow.AddMinutes(AuthorizationStateExpirationTimeInMinutes));

        var clientId = await GetClientIdAsync(authProcess.SyncMsTeams, stoppingToken);
        var authUri = BuildMicrosoftAuthUri(clientId, stateKey, authProcess.CallbackUri);

        _logger.LogInformation("Micorosoft admin consent process started. Unique state id: '{state}'", stateKey);

        return new MicrosoftAuthStartProcessResult(authUri);
    }

    public async Task HandleCallbackAsync(Guid tenant, string state, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Received admin consent callback.");

        if (!_cache.TryGetValue(state, out MicrosoftAuthProcess authProcess))
            throw new OAuthException("State does not match initialized value");

        var tenantIdKey = string.Format(MicrosoftTenantIdPattern, authProcess.ConnectorId);
        await _vault.SetSecretAsync(tenantIdKey, tenant.ToString().ToSecureString(), stoppingToken);
    }

    private async Task<SecureString> GetClientIdAsync(bool syncMsTeams, CancellationToken stoppingToken)
    {
        if (syncMsTeams == true)
        {
            _logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Teams Microsoft Enterprise Application for authorization",
                nameof(syncMsTeams), syncMsTeams);
            return await _vault.GetSecretAsync(MicrosoftClientTeamsIdKey, stoppingToken);
        }
        else
        {
            _logger.LogInformation("Network property '{PropertyName}' is set to '{Value}'. Using Basic Microsoft Enterprise Application for authorization",
                nameof(syncMsTeams), syncMsTeams);
            return await _vault.GetSecretAsync(MicrosoftClientBasicIdKey, stoppingToken);
        }
    }

    private string BuildMicrosoftAuthUri(SecureString microsoftClientId, string state, Uri callbackUrl)
    {
        _logger.LogDebug("Building microsoft admin consent path...");

        var uriBuilder = new UriBuilder("https://login.microsoftonline.com/common/adminconsent")
        {
            Query = string.Format("client_id={0}&state={1}&redirect_uri={2}", microsoftClientId.ToSystemString(), state, callbackUrl.ToString())
        };

        _logger.LogDebug("Built microsoft admin consent path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }
}