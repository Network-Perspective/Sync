using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    internal class SlackFacade : IDataSource
    {
        private readonly INetworkService _networkService;
        private readonly IMembersClient _employeeProfileClient;
        private readonly IChatClient _chatClient;
        private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<SlackFacade> _logger;

        public SlackFacade(INetworkService networkService,
                           IMembersClient employeeProfileClient,
                           IChatClient chatClient,
                           ISlackClientFacadeFactory slackClientFacadeFactory,
                           IClock clock,
                           ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _employeeProfileClient = employeeProfileClient;
            _chatClient = chatClient;
            _slackClientFacadeFactory = slackClientFacadeFactory;
            _clock = clock;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<SlackFacade>();
        }

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Fetching employees data...");

            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));
            var slackClientBotFacade = await context.EnsureSetAsync(() => Task.FromResult(_slackClientFacadeFactory.CreateWithBotToken(stoppingToken)));
            var employees = await context.EnsureSetAsync(() => _employeeProfileClient.GetEmployees(slackClientBotFacade, context.NetworkConfig.EmailFilter, stoppingToken));

            var interactionFactory = new InteractionFactory(context.HashFunction, employees);

            if (network.Properties.AutoJoinChannels)
            {
                if (network.Properties.UsesAdminPrivileges)
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

            return await _chatClient.SyncInteractionsAsync(stream, slackClientBotFacade, network, interactionFactory, timeRange, stoppingToken);
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));
            var slackClientFacade = await context.EnsureSetAsync(() => Task.FromResult(_slackClientFacadeFactory.CreateWithBotToken(stoppingToken)));

            return await _employeeProfileClient.GetHashedEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, context.HashFunction, stoppingToken);
        }

        public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
            => Task.FromResult(new EmployeeCollection(Enumerable.Empty<Employee>(), null));
    }
}