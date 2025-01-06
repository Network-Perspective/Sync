using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;

internal sealed class MicrosoftFacade(
    IUsersClient usersClient,
    IMailboxClient mailboxClient,
    ICalendarClient calendarClient,
    IChannelsClient teamsClient,
    IChatsClient chatsClient,
    IHashingService hashingService,
    ILoggerFactory loggerFactory) : IDataSource
{
    private readonly ILogger<MicrosoftFacade> _logger = loggerFactory.CreateLogger<MicrosoftFacade>();

    public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting employees for connector '{connectorId}'", context.ConnectorId);

        var connectorProperties = context.GetConnectorProperties<MicrosoftConnectorProperties>();

        var employees = await context.EnsureSetAsync(async () =>
        {
            var users = await usersClient.GetUsersAsync(context, stoppingToken);
            return EmployeesMapper.ToEmployees(users, hashingService.Hash, context.NetworkConfig.EmailFilter, connectorProperties.SyncGroupAccess);
        });

        return employees;
    }

    public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting hashed employees for connector '{connectorId}'", context.ConnectorId);

        var connectorProperties = context.GetConnectorProperties<MicrosoftConnectorProperties>();

        IEnumerable<Channel> channels = connectorProperties.SyncMsTeams == true
            ? await context.EnsureSetAsync(() => teamsClient.GetAllChannelsAsync(stoppingToken))
            : Enumerable.Empty<Channel>();

        var users = await usersClient.GetUsersAsync(context, stoppingToken);
        return HashedEmployeesMapper.ToEmployees(users, channels, hashingService.Hash, context.NetworkConfig.EmailFilter);
    }

    public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Getting interactions for connector '{connectorId}' for period {timeRange}", context.ConnectorId, context.TimeRange);
        var connectorProperties = context.GetConnectorProperties<MicrosoftConnectorProperties>();

        IEnumerable<Channel> channels = connectorProperties.SyncMsTeams == true
            ? await context.EnsureSetAsync(() => teamsClient.GetAllChannelsAsync(stoppingToken))
            : Enumerable.Empty<Channel>();

        var employees = await context.EnsureSetAsync(async () =>
        {
            var users = await usersClient.GetUsersAsync(context, stoppingToken);
            return EmployeesMapper.ToEmployees(users, hashingService.Hash, context.NetworkConfig.EmailFilter, connectorProperties.SyncGroupAccess);
        });

        var emailInteractionFactory = new EmailInteractionFactory(hashingService.Hash, employees, loggerFactory.CreateLogger<EmailInteractionFactory>());
        var meetingInteractionFactory = new MeetingInteractionFactory(hashingService.Hash, employees, loggerFactory.CreateLogger<MeetingInteractionFactory>());

        var usersEmails = employees
            .GetAllInternal()
            .Select(x => x.Id.PrimaryId);

        var resultEmails = await mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionFactory, stoppingToken);
        var resultCalendar = await calendarClient.SyncInteractionsAsync(context, stream, usersEmails, meetingInteractionFactory, stoppingToken);

        var result = SyncResult.Combine(resultEmails, resultCalendar);

        if (connectorProperties.SyncMsTeams)
        {
            var channelInteractionFactory = new ChannelInteractionFactory(hashingService.Hash, employees);
            var resultChannels = await teamsClient.SyncInteractionsAsync(context, channels, stream, channelInteractionFactory, stoppingToken);
            result = SyncResult.Combine(result, resultChannels);

            if (connectorProperties.SyncChats)
            {
                var chatInteractionFactory = new ChatInteractionFactory(hashingService.Hash, employees);
                var resultChat = await chatsClient.SyncInteractionsAsync(context, stream, usersEmails, chatInteractionFactory, stoppingToken);
                result = SyncResult.Combine(result, resultChat);
            }
        }

        _logger.LogInformation("Getting interactions for connector '{connectorId}' completed", context.ConnectorId);

        return result;
    }
}