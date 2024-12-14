using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack;

internal class UserTokenAuthHandler(IConnectorContextAccessor networkIdProvider, ICachedVault cachedSecretRepository, ILogger<AuthTokenHandler> logger)
    : AuthTokenHandler(networkIdProvider, cachedSecretRepository, SlackKeys.UserTokenKeyPattern, logger)
{
}

internal class BotTokenAuthHandler(IConnectorContextAccessor networkIdProvider, ICachedVault cachedSecretRepository, ILogger<AuthTokenHandler> logger)
    : AuthTokenHandler(networkIdProvider, cachedSecretRepository, SlackKeys.BotTokenKeyPattern, logger)
{
}

internal abstract class AuthTokenHandler(IConnectorContextAccessor connectorContextProvider, ICachedVault cachedSecretRepository, string tokenPatern, ILogger<AuthTokenHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken stoppingToken)
    {
        var token = await GetAccessTokenAsync(stoppingToken);
        SetAuthorizationHeader(ref request, token);

        var response = await base.SendAsync(request, stoppingToken);
        var responseObject = await DeserializeResponseObjectAsync(response, stoppingToken);

        if (responseObject is null)
            return response;

        if (IsTokenRevoked(responseObject))
        {
            logger.LogInformation("Response indicate the token has been revoked");
            await cachedSecretRepository.ClearCacheAsync(stoppingToken);
            return await SendAsync(request, stoppingToken);
        }

        return response;
    }

    private async Task<SecureString> GetAccessTokenAsync(CancellationToken stoppingToken)
    {
        var tokenKey = string.Format(tokenPatern, connectorContextProvider.Context.ConnectorId);
        return await cachedSecretRepository.GetSecretAsync(tokenKey, stoppingToken);
    }

    private static void SetAuthorizationHeader(ref HttpRequestMessage request, SecureString token)
        => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.ToSystemString());

    private async Task<IResponseWithError> DeserializeResponseObjectAsync(HttpResponseMessage response, CancellationToken stoppingToken)
    {
        try
        {
            var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            return JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to deserialize response to {Type}", typeof(IResponseWithError));
            return null;
        }

    }

    private static bool IsTokenRevoked(IResponseWithError response)
        => response.IsOk == false && response.Error == SlackApiErrorCodes.TokenRevoked;
}