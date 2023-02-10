using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
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
        private readonly ISlackHttpClientFactory _slackHttpClientFactory;
        private readonly CursorPaginationHandler _cursorPaginationHandler;
        private readonly IClock _clock;
        private readonly ILogger<SlackFacade> _logger;

        public SlackFacade(INetworkService networkService,
                           IMembersClient employeeProfileClient,
                           IChatClient chatClient,
                           ISlackHttpClientFactory slackHttpClientFactory,
                           CursorPaginationHandler cursorPaginationHandler,
                           IClock clock,
                           ILogger<SlackFacade> logger)
        {
            _networkService = networkService;
            _employeeProfileClient = employeeProfileClient;
            _chatClient = chatClient;
            _slackHttpClientFactory = slackHttpClientFactory;
            _cursorPaginationHandler = cursorPaginationHandler;
            _clock = clock;
            _logger = logger;
        }

        public async Task SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Fetching employees data...");

            var storagePath = Path.Combine("tmp", context.NetworkId.ToString());

            await InitializeInContext(context, () => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));
            await InitializeInContext(context, async () =>
            {
                var slackClient = await _slackHttpClientFactory.CreateAsync(context.NetworkId, stoppingToken);
                var slackClientFacade = new SlackClientFacade(slackClient, _cursorPaginationHandler);
                return slackClientFacade as ISlackClientFacade;
            });


            var slackClientFacade = context.Get<ISlackClientFacade>();
            var network = context.Get<Network<SlackNetworkProperties>>();

            await InitializeInContext(context, () => _employeeProfileClient.GetEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, stoppingToken));

            var employees = context.Get<EmployeeCollection>();

            var interactionFactory = new InteractionFactory(context.HashFunction, employees);

            var timeRange = new TimeRange(context.TimeRange.Start.AddDays(-30), _clock.UtcNow());

            await _chatClient.SyncInteractionsAsync(stream, slackClientFacade, network, interactionFactory, timeRange, stoppingToken);
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            await InitializeInContext(context, () => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));

            await InitializeInContext(context, async () =>
            {
                var slackClient = await _slackHttpClientFactory.CreateAsync(context.NetworkId, stoppingToken);
                var slackClientFacade = new SlackClientFacade(slackClient, _cursorPaginationHandler);
                return slackClientFacade as ISlackClientFacade;
            });


            var slackClientFacade = context.Get<ISlackClientFacade>();
            var network = context.Get<Network<SlackNetworkProperties>>();

            return await _employeeProfileClient.GetHashedEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, context.HashFunction, stoppingToken);
        }

        public async Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);
                var slackClient = await _slackHttpClientFactory.CreateAsync(networkId, stoppingToken);
                var slackClientFacade = new SlackClientFacade(slackClient, _cursorPaginationHandler);
                await slackClientFacade.GetCurrentUserChannels(stoppingToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Network '{networkId}' is not authorized", networkId);
                _logger.LogDebug(ex, string.Empty);
                return false;
            }
        }

        private async Task InitializeInContext<T>(SyncContext context, Func<Task<T>> initializer)
        {
            if (!context.Contains<T>())
            {
                _logger.LogDebug($"{typeof(T)} is not initialized yet in the {nameof(SyncContext)}. Initializing {typeof(T)}");
                context.Set(await initializer());
            }
            else
            {
                _logger.LogDebug($"{typeof(T)} is already initialized in {nameof(SyncContext)}");
            }
        }

        public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
            => Task.FromResult(new EmployeeCollection(Enumerable.Empty<Employee>(), null));
    }
}