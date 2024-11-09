using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Auth;

internal class AuthTokenHandler(IConnectorInfoProvider connectorInfoProvider, ICachedVault cachedSecretRepository, IJiraUnauthorizedFacade jiraFacade, ILogger<AuthTokenHandler> logger) : DelegatingHandler
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken stoppingToken)
    {
        var token = await GetAccessTokenAsync(stoppingToken);

        if (IsExpiredOrAboutToExpire(token))
            token = await RenewTokenAsync(stoppingToken);

        SetAuthorizationHeader(ref request, token);

        var response = await base.SendAsync(request, stoppingToken);

        return response;
    }

    private async Task<SecureString> RenewTokenAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Renewing access token");
        await _semaphoreSlim.WaitAsync(stoppingToken);

        try
        {
            logger.LogDebug("Fetching current access token to ensure it's current one");
            var accessToken = await GetAccessTokenAsync(stoppingToken);

            if (!IsExpiredOrAboutToExpire(accessToken))
            {
                logger.LogDebug("Current token is not expired not about to expire. Probably a refresh task completed in meantime");
                return accessToken;
            }

            var connectorId = connectorInfoProvider.Get().Id;

            var refreshTokenKey = string.Format(JiraKeys.JiraRefreshTokenPattern, connectorId);
            var accessTokenKey = string.Format(JiraKeys.JiraAccessTokenKeyPattern, connectorId);

            var getRefreshTokenTask = cachedSecretRepository.GetSecretAsync(refreshTokenKey, stoppingToken);
            var getClientIdTask = cachedSecretRepository.GetSecretAsync(JiraClientKeys.JiraClientIdKey, stoppingToken);
            var getClientSecretTask = cachedSecretRepository.GetSecretAsync(JiraClientKeys.JiraClientSecretKey, stoppingToken);

            await Task.WhenAll(getRefreshTokenTask, getClientIdTask, getClientSecretTask);

            var refreshResult = await jiraFacade.RefreshTokenFlowAsync(
                getClientIdTask.Result.ToSystemString(),
                getClientSecretTask.Result.ToSystemString(),
                getRefreshTokenTask.Result.ToSystemString(),
                stoppingToken);

            var setRefreshTokenTask = cachedSecretRepository.SetSecretAsync(refreshTokenKey, refreshResult.RefreshToken.ToSecureString(), stoppingToken);
            var setAccessTokenTask = cachedSecretRepository.SetSecretAsync(accessTokenKey, refreshResult.AccessToken.ToSecureString(), stoppingToken);

            await Task.WhenAll(setRefreshTokenTask, setAccessTokenTask);

            await cachedSecretRepository.ClearCacheAsync(stoppingToken);

            return refreshResult.AccessToken.ToSecureString();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task<SecureString> GetAccessTokenAsync(CancellationToken stoppingToken)
    {
        var connectorInfo = connectorInfoProvider.Get();
        var tokenKey = string.Format(JiraKeys.JiraAccessTokenKeyPattern, connectorInfo.Id);
        return await cachedSecretRepository.GetSecretAsync(tokenKey, stoppingToken);
    }

    private static bool IsExpiredOrAboutToExpire(SecureString accessToken)
    {
        var expirationTime = GetTokenExpiration(accessToken);

        return expirationTime <= DateTime.UtcNow;
    }

    private static DateTime GetTokenExpiration(SecureString accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(accessToken.ToSystemString());
        var expUnix = long.Parse(jwtSecurityToken.Claims.First(c => c.Type == "exp").Value);
        var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

        return expTime;
    }

    private static void SetAuthorizationHeader(ref HttpRequestMessage request, SecureString token)
        => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.ToSystemString());
}