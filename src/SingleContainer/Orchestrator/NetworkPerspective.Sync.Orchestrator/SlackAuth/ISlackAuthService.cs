using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.SlackAuth;

public interface ISlackAuthService
{
    Task<SlackAuthStartProcessResult> StartAuthProcessAsync(SlackAuthProcess authProcess, CancellationToken stoppingToken = default);
    Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
}

internal class SlackAuthService : ISlackAuthService
{
    private const int SlackAuthorizationCodeExpirationTimeInMinutes = 10;
    private const string SlackClientIdKey = "SlackClientId";
    private const string SlackClientSecretKey = "SlackClientSecret";
    private const string SlackBotTokenKeyPattern = "SlackBotToken-{0}";
    private const string SlackUserTokenKeyPattern = "SlackUserToken-{0}";
    private const string SlackRefreshTokenPattern = "SlackRefreshToken-{0}";

    private readonly IVault _vault;
    private readonly IAuthStateKeyFactory _stateKeyFactory;
    private readonly IMemoryCache _cache;
    private readonly IWorkerRouter _workerRouter;
    private readonly ISlackClientUnauthorizedFacade _slackClientUnauthorizedFacade;
    private readonly SlackAuthConfig _slackAuthConfig;
    private readonly ILogger<SlackAuthService> _logger;

    public SlackAuthService(IVault vault, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, IWorkerRouter workerRouter, IOptions<SlackAuthConfig> slackAuthConfig, ISlackClientUnauthorizedFacade slackClientUnauthorizedFacade, ILogger<SlackAuthService> logger)
    {
        _vault = vault;
        _stateKeyFactory = stateKeyFactory;
        _cache = cache;
        _workerRouter = workerRouter;
        _slackClientUnauthorizedFacade = slackClientUnauthorizedFacade;
        _slackAuthConfig = slackAuthConfig.Value;
        _logger = logger;
    }

    public async Task<SlackAuthStartProcessResult> StartAuthProcessAsync(SlackAuthProcess authProcess, CancellationToken stoppingToken = default)
    {

        _logger.LogInformation("Starting slack autentication process...");

        var clientId = await _vault.GetSecretAsync(SlackClientIdKey, stoppingToken);

        var stateKey = _stateKeyFactory.Create();
        _cache.Set(stateKey, authProcess, DateTimeOffset.UtcNow.AddMinutes(SlackAuthorizationCodeExpirationTimeInMinutes));

        var authUri = BuildSlackAuthUri(stateKey, authProcess, clientId);

        _logger.LogInformation("Slack authentication process started. Unique state id: '{state}'", stateKey);

        return new SlackAuthStartProcessResult(authUri);
    }

    public async Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Received Authentication callback.");

        if (!_cache.TryGetValue(state, out SlackAuthProcess authProcess))
            throw new OAuthException("State does not match initialized value");

        var clientId = await _vault.GetSecretAsync(SlackClientIdKey, stoppingToken);
        var clientSecret = await _vault.GetSecretAsync(SlackClientSecretKey, stoppingToken);

        var request = new OAuthAccessRequest
        {
            ClientId = clientId.ToSystemString(),
            ClientSecret = clientSecret.ToSystemString(),
            GrantType = "authorization_code",
            RedirectUri = authProcess.CallbackUri.ToString(),
            Code = code,
        };

        var response = await _slackClientUnauthorizedFacade.AccessAsync(request, stoppingToken);

        var secrets = new Dictionary<string, SecureString>();

        var botTokenKey = string.Format(SlackBotTokenKeyPattern, authProcess.ConnectorId);
        secrets.Add(botTokenKey, response.User.AccessToken.ToSecureString());

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

        await _workerRouter.SetSecretsAsync(authProcess.WorkerName, secrets);
        _logger.LogInformation("Authentication callback processed successfully. Network '{networkId}' is configured for synchronization", authProcess.ConnectorId);
    }

    private string BuildSlackAuthUri(string state, SlackAuthProcess authProcess, SecureString slackClientId)
    {
        _logger.LogDebug("Building slack auth path... The {parameter} is set to '{value}'", nameof(authProcess.RequireAdminPrivileges), authProcess.RequireAdminPrivileges);

        var uriBuilder = new UriBuilder("https://slack.com/oauth/v2/authorize");

        var scopesParameter = string.Join(',', _slackAuthConfig.Scopes);

        var userScopes = authProcess.RequireAdminPrivileges
            ? _slackAuthConfig.UserScopes.Union(_slackAuthConfig.AdminUserScopes)
            : _slackAuthConfig.UserScopes;
        var userScopesParameter = string.Join(',', userScopes);

        uriBuilder.Query = string.Format("redirect_uri={0}&client_id={1}&scope={2}&user_scope={3}&state={4}", authProcess.CallbackUri.ToString(), slackClientId.ToSystemString(), scopesParameter, userScopesParameter, state);

        _logger.LogDebug("Built slack auth path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }

}
