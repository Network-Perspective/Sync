using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal interface IUsersClient
{
    Task<IEnumerable<User>> GetUsersAsync(SyncContext contex, CancellationToken stoppingToken = default);
}

internal class UsersClient(GraphServiceClient graphClient, IGlobalStatusCache tasksStatusesCache, ILogger<UsersClient> logger) : IUsersClient
{
    private const string TaskCaption = "Synchronizing employees metadata";
    private const string TaskDescription = "Fetching employees metadata from Microsoft API";

    private const int MaxPageSize = 999;

    public async Task<IEnumerable<User>> GetUsersAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Fetching users for connector '{connectorId}'...", context.ConnectorId);

        var taskStatus = SingleTaskStatus.WithUnknownProgress(TaskCaption, TaskDescription);
        await tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);

        var result = new List<User>();

        var usersResponse = await graphClient
            .Users
            .GetAsync(x =>
            {
                x.QueryParameters = new UsersRequestBuilder.UsersRequestBuilderGetQueryParameters
                {
                    Select = new[]
                    {
                        nameof(User.Id),
                        nameof(User.Mail),
                        nameof(User.OtherMails),
                        nameof(User.EmployeeId),
                        nameof(User.DisplayName),
                        nameof(User.Department),
                        nameof(User.CreatedDateTime)
                    },
                    Filter = "userType eq 'Member'",
                    Top = MaxPageSize,
                    Expand = new[]
                    {
                        nameof(User.Manager)
                    }
                };
            }, stoppingToken);

        var pageIterator = PageIterator<User, UserCollectionResponse>
            .CreatePageIterator(graphClient, usersResponse,
            async user =>
            {
                var groups = await graphClient
                    .Users[user.Id]
                    .TransitiveMemberOf
                    .GraphGroup
                    .GetAsync(x =>
                    {
                        x.QueryParameters = new()
                        {
                            Select = new[]
                            {
                                nameof(Group.DisplayName),
                            }
                        };
                    });

                var mails = new[] { user.Mail }
                    .Union(user.OtherMails);
                var groupsNames = groups.Value.Select(x => x.DisplayName);

                if (context.NetworkConfig.EmailFilter.IsInternal(mails, groupsNames))
                    result.Add(user);

                return true;
            },
            request =>
            {
                return request;
            });

        await pageIterator.IterateAsync(stoppingToken);


        if (!result.Any())
            logger.LogWarning("No users found in connector '{connectorId}'", context.ConnectorId);
        else
            logger.LogDebug("Fetching employees for connector '{neconnectorIdtworkId}' completed. '{count}' employees found", context.ConnectorId, result.Count);

        return result;
    }
}