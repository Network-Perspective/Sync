﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack;

internal class SlackFacade : IDataSource
{
    private readonly IMembersClient _employeeProfileClient;
    private readonly IChatClient _chatClient;
    private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
    private readonly IHashingService _hashingService;
    private readonly IClock _clock;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SlackFacade> _logger;

    public SlackFacade(IMembersClient employeeProfileClient,
                       IChatClient chatClient,
                       ISlackClientFacadeFactory slackClientFacadeFactory,
                       IHashingService hashingService,
                       IClock clock,
                       ILoggerFactory loggerFactory)
    {
        _employeeProfileClient = employeeProfileClient;
        _chatClient = chatClient;
        _slackClientFacadeFactory = slackClientFacadeFactory;
        _hashingService = hashingService;
        _clock = clock;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<SlackFacade>();
    }

    public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Fetching employees data...");

        var slackClientBotFacade = await context.EnsureSetAsync(() => Task.FromResult(_slackClientFacadeFactory.CreateWithBotToken(stoppingToken)));
        var employees = await context.EnsureSetAsync(() => _employeeProfileClient.GetEmployees(slackClientBotFacade, context.NetworkConfig.EmailFilter, stoppingToken));

        var interactionFactory = new InteractionFactory(_hashingService.Hash, employees);

        var connectorProperties = new SlackConnectorProperties(context.ConnectorProperties);

        if (connectorProperties.AutoJoinChannels)
        {
            if (connectorProperties.UsesAdminPrivileges)
            {
                var slackClientAdminFacade = _slackClientFacadeFactory.CreateWithUserToken(stoppingToken);
                var joiner = new PrivilegedChatJoiner(slackClientBotFacade, slackClientAdminFacade, _loggerFactory.CreateLogger<PrivilegedChatJoiner>());
                await joiner.JoinAsync(stoppingToken);
            }
            else
            {
                var joiner = new UnprivilegedChatJoiner(slackClientBotFacade, _loggerFactory.CreateLogger<UnprivilegedChatJoiner>());
                await joiner.JoinAsync(stoppingToken);
            }
        }

        var timeRange = new TimeRange(context.TimeRange.Start.AddDays(-30), _clock.UtcNow());

        return await _chatClient.SyncInteractionsAsync(stream, slackClientBotFacade, context.ConnectorId, interactionFactory, timeRange, stoppingToken);
    }

    public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var slackClientFacade = await context.EnsureSetAsync(() => Task.FromResult(_slackClientFacadeFactory.CreateWithBotToken(stoppingToken)));

        return await _employeeProfileClient.GetHashedEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, _hashingService.Hash, stoppingToken);
    }

    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        => Task.FromResult(new EmployeeCollection(Enumerable.Empty<Employee>(), null));
}