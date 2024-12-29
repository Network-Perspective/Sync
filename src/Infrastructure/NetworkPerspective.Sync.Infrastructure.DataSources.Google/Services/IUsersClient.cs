using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal interface IUsersClient
{
    Task<IEnumerable<User>> GetUsersAsync(Guid connectorId, ICredential credentials, EmployeeFilter employeeFilter, CancellationToken stoppingToken = default);
}

internal sealed class UsersClient(IScopedStatusCache statusCache, IOptions<GoogleConfig> config, IEnumerable<ICriteria> criterias, IRetryPolicyProvider retryPolicyProvider, ILogger<UsersClient> logger) : IUsersClient
{
    private const string TaskCaption = "Synchronizing users";
    private const string TaskDescription = "Fetching users data from Google API";

    private readonly GoogleConfig _config = config.Value;

    public async Task<IEnumerable<User>> GetUsersAsync(Guid connectorId, ICredential credentials, EmployeeFilter employeeFilter, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Fetching users for connector '{connectorId}'...", connectorId);

        await statusCache.SetStatusAsync(new SingleTaskStatus(TaskCaption, TaskDescription, null), stoppingToken);

        var retryPolicy = retryPolicyProvider.GetSecretRotationRetryPolicy();
        var users = await retryPolicy.ExecuteAsync(() => GetAllGoogleUsers(credentials, stoppingToken));

        var filteredUsers = FilterUsers(employeeFilter, users);

        if (!filteredUsers.Any())
            logger.LogWarning("No users found in connector '{connectorId}'", connectorId);
        else
            logger.LogDebug("Fetching employees for network '{connectorId}' completed. '{count}' employees found", connectorId, filteredUsers.Count());

        return filteredUsers;
    }

    private async Task<IList<User>> GetAllGoogleUsers(ICredential credentials, CancellationToken stoppingToken)
    {
        const string currentAccountCustomer = "my_customer";

        var service = new DirectoryService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = _config.ApplicationName,
        });

        var result = new List<User>();
        var nextPageToken = string.Empty;
        do
        {
            var request = service.Users.List();
            request.MaxResults = 500;
            request.Customer = currentAccountCustomer;
            request.OrderBy = UsersResource.ListRequest.OrderByEnum.Email;
            request.PageToken = nextPageToken;
            request.Projection = UsersResource.ListRequest.ProjectionEnum.Full; // we do NOT know upfront what kind of custom section is set, so we cannot use ProjectionEnum.Custom
            var response = await retryPolicyProvider
                .GetThrottlingRetryPolicy()
                .ExecuteAsync(request.ExecuteAsync, stoppingToken);

            if (response.UsersValue != null)
                result.AddRange(response.UsersValue);

            nextPageToken = response.NextPageToken;

        } while (!string.IsNullOrEmpty(nextPageToken) && !stoppingToken.IsCancellationRequested);

        return result;
    }

    private IEnumerable<User> FilterUsers(EmployeeFilter emailFilter, IList<User> employeesProfiles)
    {
        foreach (var criteria in criterias)
            employeesProfiles = criteria.MeetCriteria(employeesProfiles);

        var filteredProfiles = employeesProfiles
            .Where(x => emailFilter.IsInternal(x.PrimaryEmail)
                || x.Aliases != null && x.Aliases.Any(emailFilter.IsInternal));

        return filteredProfiles;
    }
}