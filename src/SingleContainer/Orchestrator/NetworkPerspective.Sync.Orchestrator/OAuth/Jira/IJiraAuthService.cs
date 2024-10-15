using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.OAuth.Jira.Client;
using NetworkPerspective.Sync.Orchestrator.OAuth.Jira.Client.Model;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;
public interface IJiraAuthService
{
    Task<JiraAuthStartProcessResult> StartAuthProcessAsync(JiraAuthProcess authProcess, CancellationToken stoppingToken = default);
    Task HandleCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
}

public class JiraAuthService(IVault vault, IJiraClient jiraClient, IAuthStateKeyFactory stateKeyFactory, IMemoryCache cache, IWorkerRouter workerRouter, IOptions<JiraConfig> config, ILogger<JiraAuthService> logger) : IJiraAuthService
{
    private const int JiraAuthorizationCodeExpirationTimeInMinutes = 10;

    public async Task<JiraAuthStartProcessResult> StartAuthProcessAsync(JiraAuthProcess authProcess, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Starting jira autentication process...");

        var clientId = await vault.GetSecretAsync(JiraKeys.JiraClientIdKey, stoppingToken);

        var stateKey = stateKeyFactory.Create();
        cache.Set(stateKey, authProcess, DateTimeOffset.UtcNow.AddMinutes(JiraAuthorizationCodeExpirationTimeInMinutes));

        var authUri = BuildAuthUri(stateKey, authProcess, clientId);

        logger.LogInformation("Jira authentication process started. Unique state id: '{state}'", stateKey);

        return new JiraAuthStartProcessResult(authUri);
    }

    public async Task HandleCallbackAsync(string code, string state, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Received Authentication callback.");

        if (!cache.TryGetValue(state, out JiraAuthProcess authProcess))
            throw new OAuthException("State does not match initialized value");

        var tokenResponse = await jiraClient.ExchangeCodeForTokenAsync(code, authProcess.CallbackUri, stoppingToken);
        var secrets = GetSecrets(tokenResponse, authProcess.ConnectorId);

        await workerRouter.SetSecretsAsync(authProcess.WorkerName, secrets);
    }

    private string BuildAuthUri(string state, JiraAuthProcess authProcess, SecureString clientId)
    {
        logger.LogDebug("Building jira auth path...'");

        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["audience"] = "api.atlessian.com";
        queryParameters["client_id"] = clientId.ToSystemString();
        queryParameters["scope"] = string.Join(' ', config.Value.Auth.Scopes);
        queryParameters["redirect_uri"] = authProcess.CallbackUri.ToString();
        queryParameters["state"] = state;
        queryParameters["response_type"] = "code";
        queryParameters["prompt"] = "consent";

        var uriBuilder = new UriBuilder(config.Value.BaseUrl)
        {
            Path = config.Value.Auth.Path,
            Query = queryParameters.ToString()
        };

        logger.LogDebug("Built slack auth path: '{uriBuilder}'", uriBuilder);

        return uriBuilder.ToString();
    }


    private static Dictionary<string, SecureString> GetSecrets(TokenResponse tokenResponse, Guid connectorId)
    {
        var secrets = new Dictionary<string, SecureString>();

        var accessTokenKey = string.Format(JiraKeys.JiraAccessTokenKeyPattern, connectorId);
        secrets.Add(accessTokenKey, tokenResponse.AccessToken.ToSecureString());

        var refreshTokenKey = string.Format(JiraKeys.JiraRefreshTokenPattern, connectorId);
        secrets.Add(refreshTokenKey, tokenResponse.RefreshToken.ToSecureString());

        return secrets;
    }
}