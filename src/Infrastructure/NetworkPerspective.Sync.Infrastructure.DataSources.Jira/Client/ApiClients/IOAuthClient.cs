using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.ApiClients;

internal interface IOAuthClient
{
    Task<IReadOnlyCollection<AccessibleResource>> GetAccessibleResourcesAsync(CancellationToken stoppingToken = default);
    Task<OAuthTokenResponse> RefreshTokenFlowAsync(string clientId, string clientSecret, string refreshToken, CancellationToken stoppingToken = default);
}

internal class OAuthClient(IJiraHttpClient jiraHttpClient) : IOAuthClient
{
    // https://developer.atlassian.com/cloud/oauth/getting-started/refresh-tokens/
    public async Task<OAuthTokenResponse> RefreshTokenFlowAsync(string clientId, string clientSecret, string refreshToken, CancellationToken stoppingToken = default)
    {
        var path = "oauth/token";

        var request = new OAuthTokenRequest
        {
            GrantType = "refresh_token",
            ClientId = clientId,
            ClientSecret = clientSecret,
            RefreshToken = refreshToken
        };

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        return await jiraHttpClient.PostAsync<OAuthTokenResponse>(path, content, stoppingToken);
    }

    // https://developer.atlassian.com/cloud/oauth/getting-started/making-calls-to-api/#making-calls-to-api
    public async Task<IReadOnlyCollection<AccessibleResource>> GetAccessibleResourcesAsync(CancellationToken stoppingToken = default)
    {
        var path = "oauth/token/accessible-resources";

        var result = await jiraHttpClient.GetAsync<IEnumerable<AccessibleResource>>(path, stoppingToken);
        return result.ToList().AsReadOnly();
    }
}