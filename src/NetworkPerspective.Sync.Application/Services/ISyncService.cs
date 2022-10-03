using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncService
    {
        Task SyncInteractionsAsync(SyncContext context, CancellationToken stoppingToken = default);
        Task SyncUsersAsync(SyncContext context, CancellationToken stoppingToken = default);
        Task SyncEntitiesAsync(SyncContext context, CancellationToken stoppingToken = default);
        Task SyncGroupsAsync(SyncContext context, CancellationToken stoppingToken = default);
    }

    internal sealed class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly IDataSource _dataSource;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly IInteractionsFilterFactory _interactionFilterFactory;
        private readonly IStatusLogger _statusLogger;
        private readonly IClock _clock;

        public SyncService(ILogger<SyncService> logger,
                           IDataSource dataSourceFacade,
                           ISyncHistoryService syncHistoryService,
                           INetworkPerspectiveCore networkPerspectiveCore,
                           IInteractionsFilterFactory interactionFilterFactory,
                           IStatusLogger statusLogger,
                           IClock clock)
        {
            _logger = logger;
            _dataSource = dataSourceFacade;
            _syncHistoryService = syncHistoryService;
            _networkPerspectiveCore = networkPerspectiveCore;
            _interactionFilterFactory = interactionFilterFactory;
            _statusLogger = statusLogger;
            _clock = clock;
        }

        public async Task SyncUsersAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Syncing users for network '{networkId}'", context.NetworkId);

            var employees = await _dataSource.GetEmployees(context, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Received employees profiles", stoppingToken);

            await _networkPerspectiveCore.PushUsersAsync(context.AccessToken, employees, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Uploaded profiles", stoppingToken);

            _logger.LogInformation("Sync users for network '{networkId}' completed", context.NetworkId);
        }

        public async Task SyncEntitiesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Syncing entities for network '{networkId}'", context.NetworkId);

            var employees = await _dataSource.GetHashedEmployees(context, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Received hashed employees profiles", stoppingToken);

            await _networkPerspectiveCore.PushEntitiesAsync(context.AccessToken, employees, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Uploaded hashed profiles", stoppingToken);

            _logger.LogInformation("Sync entities for network '{networkId}' completed", context.NetworkId);
        }

        public async Task SyncGroupsAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Syncing groups for network '{networkId}'", context.NetworkId);

            var employees = await _dataSource.GetHashedEmployees(context, stoppingToken);

            var groups = employees
                .GetAllInternal()
                .SelectMany(x => x.Groups) // flatten groups
                .ToHashSet(Group.EqualityComparer); // only distinct values
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Received {groups.Count} groups", stoppingToken);

            await _networkPerspectiveCore.PushGroupsAsync(context.AccessToken, groups, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, "Uploaded groups", stoppingToken);

            _logger.LogInformation("Sync groups for network '{networkId}' completed", context.NetworkId);
        }

        public async Task SyncInteractionsAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Syncing interactions for network '{networkId}' for period {period}", context.NetworkId, context.CurrentRange);
            var interactions = await _dataSource.GetInteractions(context, stoppingToken);

            var filteredInteractions = _interactionFilterFactory
                .CreateInteractionsFilter(context.CurrentRange)
                .Filter(interactions);

            await _statusLogger.LogInfoAsync(context.NetworkId, $"Received {filteredInteractions.Count} Interactions", stoppingToken);

            await _networkPerspectiveCore.PushInteractionsAsync(context.AccessToken, filteredInteractions, stoppingToken);

            var syncHistoryEntry = new SyncHistoryEntry(context.NetworkId, _clock.UtcNow(), context.CurrentRange);
            await _syncHistoryService.SaveLogAsync(syncHistoryEntry, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, "Uploaded all interactions", stoppingToken);

            _logger.LogInformation("Sync interactions for network '{networkId}' for {period} completed", context.NetworkId, context.CurrentRange);
        }

    }
}