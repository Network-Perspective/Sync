using System;
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
        private readonly ILogger<SlackFacade> _logger;

        public SlackFacade(INetworkService networkService,
                           IMembersClient employeeProfileClient,
                           IChatClient chatClient,
                           ISlackClientFacadeFactory slackClientFacadeFactory,
                           IClock clock,
                           ILogger<SlackFacade> logger)
        {
            _networkService = networkService;
            _employeeProfileClient = employeeProfileClient;
            _chatClient = chatClient;
            _slackClientFacadeFactory = slackClientFacadeFactory;
            _clock = clock;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Fetching employees data...");

            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));
            var slackClientFacade = await context.EnsureSetAsync(() => _slackClientFacadeFactory.CreateWithBotTokenAsync(context.NetworkId, stoppingToken));
            var employees = await context.EnsureSetAsync(() => _employeeProfileClient.GetEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, stoppingToken));

            var interactionFactory = new InteractionFactory(context.HashFunction, employees);

            var timeRange = new TimeRange(context.TimeRange.Start.AddDays(-30), _clock.UtcNow());

            return await _chatClient.SyncInteractionsAsync(stream, slackClientFacade, network, interactionFactory, timeRange, stoppingToken);
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            var network = await context.EnsureSetAsync(() => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));
            var slackClientFacade = await context.EnsureSetAsync(() => _slackClientFacadeFactory.CreateWithBotTokenAsync(context.NetworkId, stoppingToken));

            return await _employeeProfileClient.GetHashedEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, context.HashFunction, stoppingToken);
        }

        public async Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);
                var slackClientFacade = await _slackClientFacadeFactory.CreateWithBotTokenAsync(networkId, stoppingToken);
                await slackClientFacade.GetCurrentUserChannelsAsync(stoppingToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Network '{networkId}' is not authorized", networkId);
                _logger.LogDebug(ex, string.Empty);
                return false;
            }
        }

        public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
            => Task.FromResult(new EmployeeCollection(Enumerable.Empty<Employee>(), null));
    }
}