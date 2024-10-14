using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.ApiClients;

internal interface IProjectsClient
{
    Task<GetProjectsPaginatedResponse> GetProjectsAsync(Guid cloudId, int startAt, int maxResults, CancellationToken stoppingToken = default);
}

internal class ProjectsClient(IJiraHttpClient jiraHttpClient) : IProjectsClient
{
    // https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-projects/#api-rest-api-3-project-search-get
    public async Task<GetProjectsPaginatedResponse> GetProjectsAsync(Guid cloudId, int startAt, int maxResults, CancellationToken stoppingToken = default)
    {
        var path = string.Format("ex/jira/{0}/rest/api/3/project/search", cloudId);

        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["startAt"] = startAt.ToString();
        queryParameters["maxResults"] = maxResults.ToString();
        var query = queryParameters.ToString();

        return await jiraHttpClient.GetAsync<GetProjectsPaginatedResponse>(path + "?" + query, stoppingToken);
    }
}