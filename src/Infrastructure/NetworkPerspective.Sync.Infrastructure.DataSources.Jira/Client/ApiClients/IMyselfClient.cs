using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.ApiClients;

internal interface IMyselfClient
{
    Task<JiraUser> GetCurrentUserAsync(Guid cloudId, CancellationToken stoppingToken = default);
}

internal class MyselfClient(IJiraHttpClient jiraHttpClient) : IMyselfClient
{
    public async Task<JiraUser> GetCurrentUserAsync(Guid cloudId, CancellationToken stoppingToken = default)
    {
        var path = string.Format("ex/jira/{0}/rest/api/3/myself", cloudId);

        return await jiraHttpClient.GetAsync<JiraUser>(path, stoppingToken);
    }
}