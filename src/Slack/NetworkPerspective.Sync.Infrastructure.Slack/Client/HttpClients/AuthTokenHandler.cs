using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{

    internal class UserTokenAuthHandler : AuthTokenHandler
    {
        public UserTokenAuthHandler(INetworkIdProvider networkIdProvider, ICachedSecretRepository cachedSecretRepository, ILogger<AuthTokenHandler> logger)
            : base(networkIdProvider, cachedSecretRepository, SlackKeys.UserTokenKeyPattern, logger)
        { }
    }

    internal class BotTokenAuthHandler : AuthTokenHandler
    {
        public BotTokenAuthHandler(INetworkIdProvider networkIdProvider, ICachedSecretRepository cachedSecretRepository, ILogger<AuthTokenHandler> logger)
            : base(networkIdProvider, cachedSecretRepository, SlackKeys.TokenKeyPattern, logger)
        { }
    }

    internal abstract class AuthTokenHandler : DelegatingHandler
    {
        private readonly INetworkIdProvider _networkIdProvider;
        private readonly ICachedSecretRepository _cachedSecretRepository;
        private readonly string _tokenPatern;
        private readonly ILogger<AuthTokenHandler> _logger;

        public AuthTokenHandler(INetworkIdProvider networkIdProvider, ICachedSecretRepository cachedSecretRepository, string tokenPatern, ILogger<AuthTokenHandler> logger)
        {
            _networkIdProvider = networkIdProvider;
            _cachedSecretRepository = cachedSecretRepository;
            _tokenPatern = tokenPatern;
            _logger = logger;
        }

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
                _logger.LogInformation("Response indicate the token has been revoked");
                await _cachedSecretRepository.ClearCacheAsync(stoppingToken);
                return await SendAsync(request, stoppingToken);
            }

            return response;
        }

        private async Task<SecureString> GetAccessTokenAsync(CancellationToken stoppingToken)
        {
            var networkId = _networkIdProvider.Get();
            var tokenKey = string.Format(_tokenPatern, networkId);
            return await _cachedSecretRepository.GetSecretAsync(tokenKey, stoppingToken);
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
                _logger.LogWarning(ex, "Unable to deserialize response to {Type}", typeof(IResponseWithError));
                return null;
            }

        }

        private static bool IsTokenRevoked(IResponseWithError response)
            => response.IsOk == false && response.Error == SlackApiErrorCodes.TokenRevoked;
    }
}