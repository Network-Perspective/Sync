using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Application.Scheduler
{
    [DisallowConcurrentExecution]
    internal class SyncJob : IJob
    {
        private readonly ISyncContextFactory _syncContextFactory;
        private readonly ISyncServiceFactory _syncServiceFactory;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly INetworkService _networkService;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncContextFactory syncContextFactory,
            ISyncServiceFactory syncServiceFactory,
            IStatusLoggerFactory statusLoggerFactory,
            INetworkService networkService,
            ILogger<SyncJob> logger)
        {
            _syncContextFactory = syncContextFactory;
            _syncServiceFactory = syncServiceFactory;
            _statusLoggerFactory = statusLoggerFactory;
            _networkService = networkService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var networkId = GetNetworkId(context);
            var statusLogger = _statusLoggerFactory.CreateForNetwork(networkId);

            try
            {
                using var syncContext = await _syncContextFactory.CreateForNetworkAsync(networkId, context.CancellationToken);

                await statusLogger.LogInfoAsync("Sync started", context.CancellationToken);

                _logger.LogInformation("Executing Synchronization Job for Network '{network}' for timerange '{timeRange}'", syncContext.NetworkId, syncContext.TimeRange);

                var syncService = await _syncServiceFactory.CreateAsync(syncContext.NetworkId, context.CancellationToken);

                var network = await _networkService.GetAsync<NetworkProperties>(syncContext.NetworkId, context.CancellationToken);

                if (network.Properties.SyncGroups)
                    await syncService.SyncGroupsAsync(syncContext, context.CancellationToken);
                else
                    await statusLogger.LogInfoAsync("Skipping sync groups");

                await syncService.SyncUsersAsync(syncContext, context.CancellationToken);
                await syncService.SyncEntitiesAsync(syncContext, context.CancellationToken);
                await syncService.SyncInteractionsAsync(syncContext, context.CancellationToken);

                await statusLogger.LogInfoAsync("Sync completed", context.CancellationToken);
                _logger.LogInformation("Syncronization Job Completed for Network '{network}'", syncContext.NetworkId);
            }
            catch (Exception ex) when (ex.IndicatesTaskCanceled())
            {
                _logger.LogInformation("Synchronization Job cancelled for Network '{network}'", networkId);
                await statusLogger.LogInfoAsync("Sync cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot complete synchronization job for Network {context.JobDetail.Key.Name}.\n{ex.Message}");
                _logger.LogDebug(ex, string.Empty);
                await statusLogger.LogErrorAsync("Sync failed");
            }
        }

        private Guid GetNetworkId(IJobExecutionContext context)
            => Guid.TryParse(context.JobDetail.Key.Name, out Guid networkId) ? networkId : Guid.Empty;
    }
}