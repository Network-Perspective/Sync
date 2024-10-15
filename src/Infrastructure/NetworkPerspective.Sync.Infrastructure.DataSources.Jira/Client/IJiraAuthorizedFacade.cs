using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.ApiClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;

public interface IJiraAuthorizedFacade
{
    Task<JiraUser> GetCurrentUserAsync(Guid cloudId, CancellationToken stoppingToken = default);
    Task<IReadOnlyCollection<AccessibleResource>> GetAccessibleResourcesAsync(CancellationToken stoppingToken = default);
    Task<IReadOnlyCollection<GetProjectsPaginatedResponse.SingleProject>> GetProjectsAsync(Guid cloudId, CancellationToken stoppingToken = default);
    Task<IReadOnlyCollection<GetBulkUsersPaginatedResponse.SingleUser>> GetUsersDetailsAsync(Guid cloudId, IEnumerable<string> usersIds, CancellationToken stoppingToken = default);
    Task<IReadOnlyCollection<JiraUser>> GetProjectsUsersAsync(Guid cloudId, string projectKey, CancellationToken stoppingToken = default);
}

internal class JiraAuthorizedFacade(IJiraHttpClient httpClient, PaginationHandler paginationHandler) : IJiraAuthorizedFacade
{
    private readonly UsersClient _usersClient = new(httpClient);
    private readonly ProjectsClient _projectsClient = new(httpClient);
    private readonly OAuthClient _oauthClient = new(httpClient);
    private readonly MyselfClient _myselfClient = new(httpClient);

    public Task<JiraUser> GetCurrentUserAsync(Guid cloudId, CancellationToken stoppingToken = default)
        => _myselfClient.GetCurrentUserAsync(cloudId, stoppingToken);

    public Task<IReadOnlyCollection<AccessibleResource>> GetAccessibleResourcesAsync(CancellationToken stoppingToken = default)
        => _oauthClient.GetAccessibleResourcesAsync(stoppingToken);

    public async Task<IReadOnlyCollection<GetBulkUsersPaginatedResponse.SingleUser>> GetUsersDetailsAsync(Guid cloudId, IEnumerable<string> usersIds, CancellationToken stoppingToken = default)
    {
        Task<GetBulkUsersPaginatedResponse> CallApi(int startAt, CancellationToken stoppingToken)
            => _usersClient.GetUsersDetailsAsync(cloudId, startAt, 1, usersIds, stoppingToken);

        var result = await paginationHandler.GetAllAsync<GetBulkUsersPaginatedResponse.SingleUser, GetBulkUsersPaginatedResponse>(CallApi, stoppingToken);
        return result.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<GetProjectsPaginatedResponse.SingleProject>> GetProjectsAsync(Guid cloudId, CancellationToken stoppingToken = default)
    {
        Task<GetProjectsPaginatedResponse> CallApi(int startAt, CancellationToken stoppingToken)
            => _projectsClient.GetProjectsAsync(cloudId, startAt, 1, stoppingToken);

        var result = await paginationHandler.GetAllAsync<GetProjectsPaginatedResponse.SingleProject, GetProjectsPaginatedResponse>(CallApi, stoppingToken);
        return result.ToList().AsReadOnly();
    }

    public Task<IReadOnlyCollection<JiraUser>> GetProjectsUsersAsync(Guid cloudId, string projectKey, CancellationToken stoppingToken = default)
        => _usersClient.GetProjectsUsers(cloudId, projectKey, stoppingToken);
}