using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Application.Scheduler
{
    [DisallowConcurrentExecution]
    internal class SyncJob : IJob
    {
        private readonly ISyncServiceFactory _syncServiceFactory;
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly ITokenService _tokenService;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly INetworkService _networkService;
        private readonly IClock _clock;
        private readonly IStatusLogger _statusLogger;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncServiceFactory syncServiceFactory,
            INetworkPerspectiveCore networkPerspectiveCore,
            ITokenService tokenService,
            ISyncHistoryService syncHistoryService,
            INetworkService networkService,
            IClock clock,
            IStatusLogger statusLogger,
            ILogger<SyncJob> logger)
        {
            _syncServiceFactory = syncServiceFactory;
            _networkPerspectiveCore = networkPerspectiveCore;
            _tokenService = tokenService;
            _syncHistoryService = syncHistoryService;
            _networkService = networkService;
            _clock = clock;
            _statusLogger = statusLogger;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var networkId = Guid.Empty;

            try
            {
                networkId = Guid.Parse(context.JobDetail.Key.Name);
                await _statusLogger.LogInfoAsync("Sync started", context.CancellationToken);

                using var syncContext = await InitializeContext(networkId, context.CancellationToken);

                _logger.LogInformation("Executing Syncronization Job for Network '{network}' for timerange '{timeRange}'", networkId, syncContext.TimeRange);

                var syncService = await _syncServiceFactory.CreateAsync(networkId, context.CancellationToken);

                var network = await _networkService.GetAsync<NetworkProperties>(networkId, context.CancellationToken);

                if (network.Properties.SyncGroups)
                    await syncService.SyncGroupsAsync(syncContext, context.CancellationToken);
                else
                    await _statusLogger.LogInfoAsync("Skipping sync groups");

                await syncService.SyncUsersAsync(syncContext, context.CancellationToken);
                await syncService.SyncEntitiesAsync(syncContext, context.CancellationToken);

                await _statusLogger.LogInfoAsync($"Synchronizing interactions {syncContext.TimeRange}", context.CancellationToken);

                await syncService.SyncInteractionsAsync(syncContext, context.CancellationToken);

                await _statusLogger.LogInfoAsync("Sync completed", context.CancellationToken);
                _logger.LogInformation("Syncronization Job Completed for Network '{network}'", networkId);
            }
            catch (Exception ex) when (ex.IndicatesTaskCanceled())
            {
                _logger.LogInformation("Synchronization Job cancelled for Network '{network}'", networkId);
                await _statusLogger.LogInfoAsync("Sync cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot complete synchronization job for Network {context.JobDetail.Key.Name}.\n{ex.Message}");
                _logger.LogDebug(ex, string.Empty);
                await _statusLogger.LogErrorAsync("Sync failed");
            }
        }

        private async Task<SyncContext> InitializeContext(Guid networkId, CancellationToken stoppingToken)
        {
            var token = await _tokenService.GetAsync(networkId, stoppingToken);
            var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(token, stoppingToken);
            var lastSyncedTimeStamp = await _syncHistoryService.EvaluateSyncStartAsync(networkId, stoppingToken);
            var now = _clock.UtcNow();

            var timeRange = new TimeRange(lastSyncedTimeStamp, now);

            return new SyncContext(networkId, networkConfig, token, timeRange, _statusLogger);
        }
    }
}