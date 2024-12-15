using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.ApiClients;

internal class AuthClient(ISlackHttpClient client)
{

    /// <see href="https://api.slack.com/methods/auth.teams.list"/>
    public async Task<TeamsListResponse> GetTeamsListAsync(int limit = 100, string cursor = default, CancellationToken stoppingToken = default)
    {
        var path = string.Format("auth.teams.list?limit={0}&cursor={1}", limit, cursor);

        return await client.GetAsync<TeamsListResponse>(path, stoppingToken);
    }

    /// <see href="https://api.slack.com/methods/auth.test"/>
    public async Task<TestResponse> TestAsync(SecureString token, CancellationToken stoppingToken = default)
    {
        const string path = "auth.test";

        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.ToSystemString());

        return await client.SendAsync<TestResponse>(request, stoppingToken);
    }

    /// <see href="https://api.slack.com/methods/auth.test"/>
    public async Task<TestResponse> TestAsync(CancellationToken stoppingToken = default)
    {
        const string path = "auth.test";

        return await client.PostAsync<TestResponse>(path, stoppingToken);
    }
}