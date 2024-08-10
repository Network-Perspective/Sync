using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.ApiClients
{
    internal class OAuthClient
    {
        private readonly ISlackHttpClient _client;

        public OAuthClient(ISlackHttpClient client)
        {
            _client = client;
        }

        public async Task<OAuthAccessResponse> AccessAsync(OAuthAccessRequest request, CancellationToken stoppingToken = default)
        {
            var path = "oauth.v2.access";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", request.ClientId),
                new KeyValuePair<string, string>("client_secret", request.ClientSecret),
                new KeyValuePair<string, string>("code", request.Code),
                new KeyValuePair<string, string>("grant_type", request.GrantType),
                new KeyValuePair<string, string>("redirect_uri", request.RedirectUri),
                new KeyValuePair<string, string>("refresh_token", request.RefreshToken)
            });

            return await _client.PostAsync<OAuthAccessResponse>(path, content, stoppingToken);
        }

        public async Task<OAuthExchangeResponse> ExchangeLegacyTokenAsync(OAuthExchangeRequest request, CancellationToken stoppingToken = default)
        {
            var path = "oauth.v2.exchange";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", request.ClientId),
                new KeyValuePair<string, string>("client_secret", request.ClientSecret),
                new KeyValuePair<string, string>("token", request.AccessToken),
            });

            return await _client.PostAsync<OAuthExchangeResponse>(path, content, stoppingToken);
        }
    }
}