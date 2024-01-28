using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{
    internal class AuthTokenHandler : DelegatingHandler
    {
        private readonly INetworkIdProvider _networkIdProvider;
        private readonly ICachedSecretRepository _cachedSecretRepository;
        private readonly string _tokenPatern;

        public AuthTokenHandler(INetworkIdProvider networkIdProvider, ICachedSecretRepository cachedSecretRepository, string tokenPatern)
        {
            _networkIdProvider = networkIdProvider;
            _cachedSecretRepository = cachedSecretRepository;
            _tokenPatern = tokenPatern;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var networkId = _networkIdProvider.Get();
            var tokenKey = string.Format(_tokenPatern, networkId);
            var token = await _cachedSecretRepository.GetSecretAsync(tokenKey, cancellationToken);

            await Task.Delay(2000);

            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.ToSystemString());


            var response = await base.SendAsync(request, cancellationToken);


            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);

            if (responseObject.IsOk == false && responseObject.Error == SlackApiErrorCodes.TokenRevoked)
            {
                await _cachedSecretRepository.ClearCacheAsync(cancellationToken);
                return await SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}