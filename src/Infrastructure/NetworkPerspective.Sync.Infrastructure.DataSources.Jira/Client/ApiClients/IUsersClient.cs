using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.ApiClients;

public interface IUsersClient
{
    Task<GetBulkUsersPaginatedResponse> GetUsersDetailsAsync(Guid cloudId, int startAt, int maxResults, IEnumerable<string> usersIds, CancellationToken stoppingToken = default);
    Task<IReadOnlyCollection<JiraUser>> GetProjectsUsers(Guid cloudId, string projectKey, CancellationToken stoppingToken = default);
}

internal class UsersClient(IJiraHttpClient jiraHttpClient) : IUsersClient
{
    // https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-users/#api-rest-api-3-user-bulk-get
    public async Task<GetBulkUsersPaginatedResponse> GetUsersDetailsAsync(Guid cloudId, int startAt, int maxResults, IEnumerable<string> usersIds, CancellationToken stoppingToken = default)
    {
        var queryStringBuilder = new StringBuilder();
        queryStringBuilder.Append($"ex/jira/{cloudId}/rest/api/3/user/bulk");
        queryStringBuilder.Append('?');
        queryStringBuilder.Append($"startAt={startAt}");
        queryStringBuilder.Append('&');
        queryStringBuilder.Append($"maxResults={maxResults}");

        foreach (var userId in usersIds)
            queryStringBuilder.Append($"&accountId={userId}");

        return await jiraHttpClient.GetAsync<GetBulkUsersPaginatedResponse>(queryStringBuilder.ToString(), stoppingToken);
    }

    // https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-user-search/#api-rest-api-3-user-assignable-multiprojectsearch-get
    public async Task<IReadOnlyCollection<JiraUser>> GetProjectsUsers(Guid cloudId, string projectKey, CancellationToken stoppingToken = default)
    {
        var path = string.Format("ex/jira/{0}/rest/api/3/user/assignable/multiProjectSearch", cloudId);

        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["projectKeys"] = projectKey;
        queryParameters["maxResults"] = int.MaxValue.ToString();
        var query = queryParameters.ToString();

        return await jiraHttpClient.GetAsync<IReadOnlyCollection<JiraUser>>(path + "?" + query, stoppingToken);
    }
}