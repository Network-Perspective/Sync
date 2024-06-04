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
        Task SyncAsync(SyncContext syncContext, CancellationToken stoppingToken = default);
    }

    internal sealed class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly IDataSource _dataSource;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly IInteractionsFilterFactory _interactionFilterFactory;
        private readonly IClock _clock;

        public SyncService(ILogger<SyncService> logger,
                           IDataSource dataSourceFacade,
                           ISyncHistoryService syncHistoryService,
                           INetworkPerspectiveCore networkPerspectiveCore,
                           IInteractionsFilterFactory interactionFilterFactory,
                           IClock clock)
        {
            _logger = logger;
            _dataSource = dataSourceFacade;
            _syncHistoryService = syncHistoryService;
            _networkPerspectiveCore = networkPerspectiveCore;
            _interactionFilterFactory = interactionFilterFactory;
            _clock = clock;
        }

        public async Task SyncAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            try
            {
                await context.StatusLogger.LogInfoAsync("Sync started", stoppingToken);
                _logger.LogInformation("Executing synchronization for Connector '{connectorId}' for timerange '{timeRange}'", context.ConnectorId, context.TimeRange);

                await _networkPerspectiveCore.ReportSyncStartAsync(context.AccessToken, context.TimeRange, stoppingToken);

                if (context.NetworkProperties.SyncGroups)
                    await SyncGroupsAsync(context, stoppingToken);
                else
                    await context.StatusLogger.LogInfoAsync("Skipping sync groups", stoppingToken);

                await SyncUsersAsync(context, stoppingToken);
                await SyncEntitiesAsync(context, stoppingToken);
                await SyncInteractionsAsync(context, stoppingToken);

                await _networkPerspectiveCore.ReportSyncSuccessfulAsync(context.AccessToken, context.TimeRange, stoppingToken);
                await context.StatusLogger.LogInfoAsync("Sync completed", stoppingToken);
                _logger.LogInformation("Synchronization completed for Connector '{connectorId}'", context.ConnectorId);
            }
            catch (Exception ex) when (ex.IndicatesTaskCanceled())
            {
                await _networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.TimeRange, ex.Message, stoppingToken);
                _logger.LogInformation("Synchronization Job cancelled for Connector '{connectorId}'", context.ConnectorId);
                await context.StatusLogger.LogInfoAsync("Sync cancelled", CancellationToken.None);
            }
            catch (Exception ex)
            {
                await _networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.TimeRange, ex.Message, stoppingToken);
                _logger.LogError(ex, "Cannot complete synchronization job for Connector {connectorId}.\n{exceptionMessage}", context.ConnectorId, ex.Message);
                await context.StatusLogger.LogErrorAsync("Sync failed", CancellationToken.None);
            }
        }

        private async Task SyncUsersAsync(SyncContext context, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Synchronizing employees profiles for connector '{connectorId}'", context.ConnectorId);
            await context.StatusLogger.LogInfoAsync($"Synchronizing employees profiles...", stoppingToken);

            var employees = await _dataSource.GetEmployeesAsync(context, stoppingToken);
            await context.StatusLogger.LogInfoAsync($"Received employees profiles", stoppingToken);
            await _networkPerspectiveCore.PushUsersAsync(context.AccessToken, employees, stoppingToken);
            await context.StatusLogger.LogInfoAsync($"Uploaded employees profiles", stoppingToken);

            await context.StatusLogger.LogInfoAsync($"Synchronization of employees profiles completed", stoppingToken);
            _logger.LogInformation("Synchronization of employees profiles for connector '{connectorId}' completed", context.ConnectorId);
        }

        private async Task SyncEntitiesAsync(SyncContext context, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Synchronizing hashed employees profiles for connector '{connectorId}'", context.ConnectorId);
            await context.StatusLogger.LogInfoAsync($"Synchronizing hashed employees profiles...", stoppingToken);

            var employees = await _dataSource.GetHashedEmployeesAsync(context, stoppingToken);
            await context.StatusLogger.LogInfoAsync($"Received hashed employees profiles", stoppingToken);
            await _networkPerspectiveCore.PushEntitiesAsync(context.AccessToken, employees, context.TimeRange.Start, stoppingToken);
            await context.StatusLogger.LogInfoAsync($"Uploaded hashed employees profiles", stoppingToken);

            await context.StatusLogger.LogInfoAsync($"Synchronization of hashed employees profiles completed", stoppingToken);
            _logger.LogInformation("Synchronization of hashed employees profiles for connector '{connectorId}' completed", context.ConnectorId);
        }

        private async Task SyncGroupsAsync(SyncContext context, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Synchronizing groups for connector '{connectorId}'", context.ConnectorId);
            await context.StatusLogger.LogInfoAsync($"Synchronizing groups...", stoppingToken);

            var employees = await _dataSource.GetHashedEmployeesAsync(context, stoppingToken);

            var groups = employees
                .GetAllInternal()
                .SelectMany(x => x.Groups)          // flatten groups
                .ToHashSet(Group.EqualityComparer); // only distinct values

            if (!context.NetworkProperties.SyncChannelsNames)
            {
                groups = groups
                    .Where(x => x.Category != Group.ChannelCategory)
                    .ToHashSet(Group.EqualityComparer);
            }

            await context.StatusLogger.LogInfoAsync($"Received {groups.Count} groups", stoppingToken);

            await _networkPerspectiveCore.PushGroupsAsync(context.AccessToken, groups, stoppingToken);
            await context.StatusLogger.LogInfoAsync("Uploaded groups", stoppingToken);

            await context.StatusLogger.LogInfoAsync($"Synchronization of groups completed", stoppingToken);
            _logger.LogInformation("Synchronization of groups for connector '{connectorId}' completed", context.ConnectorId);
        }

        private async Task SyncInteractionsAsync(SyncContext context, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Synchronizing interactions for connector '{connectorId}' for period {period}", context.ConnectorId, context.TimeRange);
            await context.StatusLogger.LogInfoAsync($"Synchronizing interactions for period '{context.TimeRange}'...", stoppingToken);

            var filter = _interactionFilterFactory
                .CreateInteractionsFilter(context.TimeRange);

            var stream = _networkPerspectiveCore.OpenInteractionsStream(context.AccessToken, stoppingToken);
            await using var filteredStream = new FilteredInteractionStreamDecorator(stream, filter);

            var result = await _dataSource.SyncInteractionsAsync(filteredStream, context, stoppingToken);

            var syncHistoryEntry = SyncHistoryEntry.CreateWithResult(context.ConnectorId, _clock.UtcNow(), context.TimeRange, result);
            await _syncHistoryService.SaveLogAsync(syncHistoryEntry, stoppingToken);

            await context.StatusLogger.LogInfoAsync($"Synchronization of interactions for period '{context.TimeRange}' completed", stoppingToken);
            _logger.LogInformation("Synchronization of interactions for connector '{connectorId}' for {period} completed{newLine}{result}", context.ConnectorId, context.TimeRange, Environment.NewLine, result);

            foreach (var ex in result.Exceptions)
                _logger.LogWarning(ex, "Task thrown exception");
        }
    }
}