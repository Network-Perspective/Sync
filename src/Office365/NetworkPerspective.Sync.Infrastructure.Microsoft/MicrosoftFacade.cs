using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal sealed class MicrosoftFacade : IDataSource
    {
        private readonly INetworkService _networkService;
        private readonly IUsersClient _usersClient;
        private readonly IMailboxClient _mailboxClient;
        private readonly ICalendarClient _calendarClient;
        private readonly IChannelsClient _channelsClient;
        private readonly IChatClient _chatClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MicrosoftFacade> _logger;

        public MicrosoftFacade(
            INetworkService networkService,
            IUsersClient usersClient,
            IMailboxClient mailboxClient,
            ICalendarClient calendarClient,
            IChannelsClient teamsClient,
            IChatClient chatClient,
            ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _usersClient = usersClient;
            _mailboxClient = mailboxClient;
            _calendarClient = calendarClient;
            _channelsClient = teamsClient;
            _chatClient = chatClient;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MicrosoftFacade>();
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users, context.NetworkConfig.EmailFilter);
            });

            return employees;
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting hashed employees for network '{networkId}'", context.NetworkId);

            var users = await _usersClient.GetUsersAsync(context, stoppingToken);
            return HashedEmployeesMapper.ToEmployees(users, context.HashFunction, context.NetworkConfig.EmailFilter);

        }

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting interactions for network '{networkId}' for period {timeRange}", context.NetworkId, context.TimeRange);
            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<MicrosoftNetworkProperties>(context.NetworkId, stoppingToken));
            var employees = await context.EnsureSetAsync(async () =>
            {
                var users = await _usersClient.GetUsersAsync(context, stoppingToken);
                return EmployeesMapper.ToEmployees(users, context.NetworkConfig.EmailFilter);
            });

            var emailInteractionfactory = new EmailInteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<EmailInteractionFactory>());
            var meetingInteractionfactory = new MeetingInteractionFactory(context.HashFunction, employees, _loggerFactory.CreateLogger<MeetingInteractionFactory>());

            var usersEmails = employees
                .GetAllInternal()
                .Select(x => x.Id.PrimaryId);

            var resultEmails = await _mailboxClient.SyncInteractionsAsync(context, stream, usersEmails, emailInteractionfactory, stoppingToken);
            var resultCalendar = await _calendarClient.SyncInteractionsAsync(context, stream, usersEmails, meetingInteractionfactory, stoppingToken);

            var result = SyncResult.Combine(resultEmails, resultCalendar);

            if (network.Properties.SyncMsTeams)
            {
                var resultChannels = await _channelsClient.SyncInteractionsAsync(context, stream, usersEmails, stoppingToken);
                var resultChat = await _chatClient.SyncInteractionsAsync(context, stream, usersEmails, stoppingToken);
                result = SyncResult.Combine(result, resultChannels, resultChat);
            }


            _logger.LogInformation("Getting interactions for network '{networkId}' completed", context.NetworkId);

            return result;
        }
    }
}