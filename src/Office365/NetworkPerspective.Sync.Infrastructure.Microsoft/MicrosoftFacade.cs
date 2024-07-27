using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal sealed class MicrosoftFacade : IDataSource
    {
        private readonly IUsersClient _usersClient;
        private readonly IMailboxClient _mailboxClient;
        private readonly ICalendarClient _calendarClient;
        private readonly IChannelsClient _channelsClient;
        private readonly IChatsClient _chatsClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MicrosoftFacade> _logger;

        public MicrosoftFacade(
            IUsersClient usersClient,
            IMailboxClient mailboxClient,
            ICalendarClient calendarClient,
            IChannelsClient teamsClient,
            IChatsClient chatsClient,
            ILoggerFactory loggerFactory)
        {
            _usersClient = usersClient;
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _channelsClient = teamsClient;
            _chatsClient = chatsClient;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MicrosoftFacade>();
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for connector '{connectorId}'", context.ConnectorId);

            var connectorProperties = context.GetConnectorProperties<MicrosoftNetworkProperties>();

            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users, context.HashFunction, context.NetworkConfig.EmailFilter, connectorProperties.SyncGroupAccess);
            });

            return employees;
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for connector '{connectorId}'", context.ConnectorId);

            var connectorProperties = context.GetConnectorProperties<MicrosoftNetworkProperties>();

            IEnumerable<Models.Channel> channels = connectorProperties.SyncMsTeams == true
                ? await context.EnsureSetAsync(() => _channelsClient.GetAllChannelsAsync(stoppingToken))
                : Enumerable.Empty<Models.Channel>();

            var users = await _usersClient.GetUsersAsync(context, stoppingToken);
            return HashedEmployeesMapper.ToEmployees(users, channels, context.HashFunction, context.NetworkConfig.EmailFilter);
        }

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for connector '{connectorId}' for period {timeRange}", context.ConnectorId, context.TimeRange);
            var connectorProperties = context.GetConnectorProperties<MicrosoftNetworkProperties>();

            IEnumerable<Models.Channel> channels = connectorProperties.SyncMsTeams == true
                ? await context.EnsureSetAsync(() => _channelsClient.GetAllChannelsAsync(stoppingToken))
                : Enumerable.Empty<Models.Channel>();

            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users, context.HashFunction, context.NetworkConfig.EmailFilter, connectorProperties.SyncGroupAccess);
            });

            var emailInteractionFactory = new EmailInteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<EmailInteractionFactory>());
            var meetingInteractionFactory = new MeetingInteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<MeetingInteractionFactory>());

            var usersEmails = employees
                .GetAllInternal()
                .Select(x => x.Id.PrimaryId);

            var resultEmails = await _mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionFactory, stoppingToken);
            var resultCalendar = await _calendarClient.SyncInteractionsAsync(context, stream, usersEmails, meetingInteractionFactory, stoppingToken);

            var result = SyncResult.Combine(resultEmails, resultCalendar);

            if (connectorProperties.SyncMsTeams)
            {
                var channelInteractionFactory = new ChannelInteractionFactory(context.HashFunction, employees);
                var resultChannels = await _channelsClient.SyncInteractionsAsync(context, channels, stream, channelInteractionFactory, stoppingToken);
                result = SyncResult.Combine(result, resultChannels);

                if (connectorProperties.SyncChats)
                {
                    var chatInteractionFactory = new ChatInteractionFactory(context.HashFunction, employees);
                    var resultChat = await _chatsClient.SyncInteractionsAsync(context, stream, usersEmails, chatInteractionFactory, stoppingToken);
                    result = SyncResult.Combine(result, resultChat);
                }
            }

            _logger.LogInformation("Getting interactions for connector '{connectorId}' completed", context.ConnectorId);

            return result;
        }
    }
}