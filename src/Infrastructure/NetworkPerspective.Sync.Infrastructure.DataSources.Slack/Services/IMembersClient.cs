using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal interface IMembersClient
{
    Task<EmployeeCollection> GetEmployees(ISlackClientBotScopeFacade slackClientFacade, EmployeeFilter emailFilter, CancellationToken stoppingToken = default);
    Task<EmployeeCollection> GetHashedEmployees(ISlackClientBotScopeFacade slackClientFacade, EmployeeFilter emailFilter, HashFunction.Delegate hashFunc, CancellationToken stoppingToken = default);
}

internal class MembersClient(ITasksStatusesCache tasksStatusesCache, IConnectorContextAccessor connectorContextProvider, ILogger<MembersClient> logger) : IMembersClient
{
    private const string TaskCaption = "Synchronizing employees metadata";
    private const string TaskDescription = "Fetching employees metadata from Slack API";

    private const string SlackBotId = "USLACKBOT";

    public Task<EmployeeCollection> GetEmployees(ISlackClientBotScopeFacade slackClientFacade, EmployeeFilter emailFilter, CancellationToken stoppingToken = default)
        => GetEmployeesInternalAsync(slackClientFacade, emailFilter, null, stoppingToken);

    public Task<EmployeeCollection> GetHashedEmployees(ISlackClientBotScopeFacade slackClientFacade, EmployeeFilter emailFilter, HashFunction.Delegate hashFunc, CancellationToken stoppingToken = default)
        => GetEmployeesInternalAsync(slackClientFacade, emailFilter, hashFunc, stoppingToken);

    private async Task<EmployeeCollection> GetEmployeesInternalAsync(ISlackClientBotScopeFacade slackClientFacade, EmployeeFilter emailFilter, HashFunction.Delegate hashFunc, CancellationToken stoppingToken = default)
    {
        var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, 0);
        await tasksStatusesCache.SetStatusAsync(connectorContextProvider.Context.ConnectorId, taskStatus, stoppingToken);

        if (hashFunc == null)
            logger.LogDebug("Fetching employees... Skipping hashing due to null {func}", nameof(hashFunc));
        else
            logger.LogDebug("Fetching employees...");

        var teams = await slackClientFacade.GetTeamsListAsync(stoppingToken);

        var allSlackUsers = new HashSet<UsersListResponse.SingleUser>();

        foreach (var team in teams)
        {
            var singleWorkspaceUsers = await slackClientFacade.GetAllUsersAsync(team.Id, stoppingToken);
            allSlackUsers = allSlackUsers.UnionBy(singleWorkspaceUsers, x => x.Id).ToHashSet();
        }

        var slackUsers = allSlackUsers
            .Where(x => emailFilter.IsInternal(x.Profile.Email))
            .Where(x => x.IsBot == false)
            .Where(x => x.Id != SlackBotId); // please see https://stackoverflow.com/questions/40679819

        var botsIds = allSlackUsers
            .Where(x => x.IsBot == true || x.Id == SlackBotId)
            .Select(x => x.Id)
            .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        if (!slackUsers.Any())
            logger.LogWarning("No employees found");
        else
            logger.LogDebug("Fetching organization members completed (employees: '{employeesCount}', bots: '{botsCount}')", slackUsers.Count(), botsIds.Count);

        var employees = new List<Employee>();

        foreach (var team in teams)
        {
            foreach (var slackUser in slackUsers)
            {
                var usersChannels = await slackClientFacade.GetAllUsersChannelsAsync(team.Id, slackUser.Id, stoppingToken);
                var groups = usersChannels.Select(x => Group.Create(x.Id, x.Name, Group.ChannelCategory));
                var employeeId = EmployeeId.Create(slackUser.Profile.Email, slackUser.Id);
                var employee = Employee.CreateInternal(employeeId, groups);
                employees.Add(employee);
                logger.LogTrace("User: '{email}'", slackUser.Profile.Email);
            }
        }

        foreach (var botId in botsIds)
        {
            var bot = Employee.CreateBot(botId);
            employees.Add(bot);
        }

        return new EmployeeCollection(employees, hashFunc);
    }
}