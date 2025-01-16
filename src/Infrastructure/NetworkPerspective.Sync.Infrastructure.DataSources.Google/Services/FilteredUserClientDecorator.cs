using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal class FilteredUserClientDecorator(IUsersClient usersClient, ISyncContextAccessor syncContextAccessor, IEnumerable<ICriteria> criterias, ILogger<FilteredUserClientDecorator> logger) : IUsersClient
{
    public async Task<IEnumerable<User>> GetUsersAsync(ICredential credentials, CancellationToken stoppingToken = default)
    {
        var syncContext = syncContextAccessor.SyncContext;
        var users = await usersClient.GetUsersAsync(credentials, stoppingToken);

        var filteredUsers = FilterUsers(syncContext.NetworkConfig.EmailFilter, users.ToList());

        if (!filteredUsers.Any())
            logger.LogWarning("No users found in connector '{connectorId}'", syncContext.ConnectorId);
        else
            logger.LogDebug("Fetching employees for network '{connectorId}' completed. '{count}' employees found", syncContext.ConnectorId, filteredUsers.Count());

        return filteredUsers;
    }

    private IEnumerable<User> FilterUsers(EmployeeFilter emailFilter, IList<User> users)
    {
        foreach (var criteria in criterias)
            users = criteria.MeetCriteria(users);

        var filteredProfiles = users
            .Where(x => emailFilter.IsInternal(x.PrimaryEmail)
                || x.Aliases != null && x.Aliases.Any(emailFilter.IsInternal));

        return filteredProfiles;
    }
}