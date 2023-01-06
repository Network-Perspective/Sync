using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    internal class SlackFacade : IDataSource
    {
        private readonly INetworkService _networkService;
        private readonly ISecretRepository _secretRepository;
        private readonly IMembersClient _employeeProfileClient;
        private readonly IChatClient _chatClient;
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CursorPaginationHandler _cursorPaginationHandler;
        private readonly IClock _clock;
        private readonly ILogger<SlackFacade> _logger;

        public SlackFacade(INetworkService networkService,
                           ISecretRepository secretRepository,
                           IMembersClient employeeProfileClient,
                           IChatClient chatClient,
                           IHashingServiceFactory hashingServiceFactory,
                           IHttpClientFactory httpClientFactory,
                           CursorPaginationHandler cursorPaginationHandler,
                           IClock clock,
                           ILogger<SlackFacade> logger)
        {
            _networkService = networkService;
            _secretRepository = secretRepository;
            _employeeProfileClient = employeeProfileClient;
            _chatClient = chatClient;
            _hashingServiceFactory = hashingServiceFactory;
            _httpClientFactory = httpClientFactory;
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
                var tokenKey = string.Format(SlackKeys.TokenKeyPattern, context.NetworkId);
                var token = await _secretRepository.GetSecretAsync(tokenKey, stoppingToken);
                var slackClientFacade = new SlackClientFacade(_httpClientFactory, _cursorPaginationHandler);
                slackClientFacade.SetAccessToken(token.ToSystemString());
                return slackClientFacade as ISlackClientFacade;
            });

            await InitializeInContext(context, () => _hashingServiceFactory.CreateAsync(_secretRepository, stoppingToken));

            var slackClientFacace = context.Get<ISlackClientFacade>();
            var hashingService = context.Get<IHashingService>();
            var network = context.Get<Network<SlackNetworkProperties>>();

            await InitializeInContext(context, () => _employeeProfileClient.GetEmployees(slackClientFacace, context.NetworkConfig.EmailFilter, stoppingToken));

            var employees = context.Get<EmployeeCollection>();

            var interactionFactory = new InteractionFactory(hashingService.Hash, employees);

            var timeRange = new TimeRange(context.Since.AddDays(-30), _clock.UtcNow());

            await _chatClient.SyncInteractionsAsync(stream, slackClientFacace, network, interactionFactory, timeRange, stoppingToken);
        }

        public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            await InitializeInContext(context, () => _networkService.GetAsync<SlackNetworkProperties>(context.NetworkId, stoppingToken));

            await InitializeInContext(context, async () =>
            {
                var tokenKey = string.Format(SlackKeys.TokenKeyPattern, context.NetworkId);
                var token = await _secretRepository.GetSecretAsync(tokenKey, stoppingToken);
                var slackClientFacade = new SlackClientFacade(_httpClientFactory, _cursorPaginationHandler);
                slackClientFacade.SetAccessToken(token.ToSystemString());
                return slackClientFacade as ISlackClientFacade;
            });

            await InitializeInContext(context, () => _hashingServiceFactory.CreateAsync(_secretRepository, stoppingToken));

            var slackClientFacade = context.Get<ISlackClientFacade>();
            var hashingService = context.Get<IHashingService>();
            var network = context.Get<Network<SlackNetworkProperties>>();

            return await _employeeProfileClient.GetHashedEmployees(slackClientFacade, context.NetworkConfig.EmailFilter, hashingService.Hash, stoppingToken);
        }

        public async Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);
                var tokenKey = string.Format(SlackKeys.TokenKeyPattern, networkId);
                var token = await _secretRepository.GetSecretAsync(tokenKey, stoppingToken);
                using var slackClientFacade = new SlackClientFacade(_httpClientFactory, _cursorPaginationHandler);
                slackClientFacade.SetAccessToken(token.ToSystemString());
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