using System;
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

            var employees = await _dataSource.GetEmployeesAsync(context, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Received employees profiles", stoppingToken);

            await _networkPerspectiveCore.PushUsersAsync(context.AccessToken, employees, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Uploaded profiles", stoppingToken);

            _logger.LogInformation("Sync users for network '{networkId}' completed", context.NetworkId);
        }

        public async Task SyncEntitiesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Syncing entities for network '{networkId}'", context.NetworkId);

            var employees = await _dataSource.GetHashedEmployeesAsync(context, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Received hashed employees profiles", stoppingToken);

            await _networkPerspectiveCore.PushEntitiesAsync(context.AccessToken, employees, context.Since, stoppingToken);
            await _statusLogger.LogInfoAsync(context.NetworkId, $"Uploaded hashed profiles", stoppingToken);

            _logger.LogInformation("Sync entities for network '{networkId}' completed", context.NetworkId);
        }

        public async Task SyncGroupsAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Syncing groups for network '{networkId}'", context.NetworkId);

            var employees = await _dataSource.GetHashedEmployeesAsync(context, stoppingToken);

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
            try
            {
                _logger.LogInformation("Syncing interactions for network '{networkId}' for period {period}", context.NetworkId, context.CurrentRange);

                await _networkPerspectiveCore.ReportSyncStartAsync(context.AccessToken, context.CurrentRange, stoppingToken);

                var filter = _interactionFilterFactory
                    .CreateInteractionsFilter(context.CurrentRange);

                var stream = _networkPerspectiveCore.OpenInteractionsStream(context.AccessToken, stoppingToken);
                await using var filteredStream = new FilteredInteractionStreamDecorator(stream, filter);

                await _dataSource.SyncInteractionsAsync(stream, context, stoppingToken);

                //await _statusLogger.LogInfoAsync(context.NetworkId, $"Received {filteredInteractions.Count} Interactions", stoppingToken);

                await _networkPerspectiveCore.ReportSyncSuccessfulAsync(context.AccessToken, context.CurrentRange, stoppingToken);

                var syncHistoryEntry = new SyncHistoryEntry(context.NetworkId, _clock.UtcNow(), context.CurrentRange);
                await _syncHistoryService.SaveLogAsync(syncHistoryEntry, stoppingToken);

                await _statusLogger.LogInfoAsync(context.NetworkId, "Synced interactions", stoppingToken);

                _logger.LogInformation("Sync interactions for network '{networkId}' for {period} completed", context.NetworkId, context.CurrentRange);
            }
            catch (Exception ex)
            {
                await _networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.CurrentRange, ex.Message, stoppingToken);
                throw;
            }
        }

    }
}