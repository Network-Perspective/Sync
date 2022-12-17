using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache;
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
        private readonly IInteractionsCacheFactory _interactionsCacheFactory;
        private readonly IStatusLogger _statusLogger;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncServiceFactory syncServiceFactory,
            INetworkPerspectiveCore networkPerspectiveCore,
            ITokenService tokenService,
            ISyncHistoryService syncHistoryService,
            INetworkService networkService,
            IClock clock,
            IInteractionsCacheFactory interactionsCacheFactory,
            IStatusLogger statusLogger,
            ILogger<SyncJob> logger)
        {
            _syncServiceFactory = syncServiceFactory;
            _networkPerspectiveCore = networkPerspectiveCore;
            _tokenService = tokenService;
            _syncHistoryService = syncHistoryService;
            _networkService = networkService;
            _clock = clock;
            _interactionsCacheFactory = interactionsCacheFactory;
            _statusLogger = statusLogger;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var networkId = Guid.Empty;

            try
            {
                networkId = Guid.Parse(context.JobDetail.Key.Name);
                await _statusLogger.LogInfoAsync(networkId, "Sync started", context.CancellationToken);

                var token = await _tokenService.GetAsync(networkId, context.CancellationToken);
                var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(token, context.CancellationToken);
                var lastSyncedTimeStamp = await _syncHistoryService.EvaluateSyncStartAsync(networkId, context.CancellationToken);
                var now = _clock.UtcNow();

                var interactionsCache = await _interactionsCacheFactory.CreateAsync(networkId, context.CancellationToken);
                using var syncContext = new SyncContext(networkId, networkConfig, token, lastSyncedTimeStamp, now);

                _logger.LogInformation("Executing Syncronization Job for Network '{network}' starting from {timestamp}", networkId, lastSyncedTimeStamp.ToShortDateString());

                var network = await _networkService.GetAsync<NetworkProperties>(networkId, context.CancellationToken);
                var syncService = await _syncServiceFactory.CreateAsync(networkId, context.CancellationToken);

                if (network.Properties.SyncGroups)
                    await syncService.SyncGroupsAsync(syncContext, context.CancellationToken);
                else
                    await _statusLogger.LogInfoAsync(networkId, "Skipping sync groups");

                await syncService.SyncUsersAsync(syncContext, context.CancellationToken);
                await syncService.SyncEntitiesAsync(syncContext, context.CancellationToken);

                do
                {
                    await _statusLogger.LogInfoAsync(networkId, $"Synchronizing interactions {syncContext.CurrentRange}", context.CancellationToken);

                    await syncService.SyncInteractionsAsync(syncContext, context.CancellationToken);
                    lastSyncedTimeStamp = syncContext.CurrentRange.End;
                    now = _clock.UtcNow();
                    syncContext.MoveToNextSyncRange(now);
                } while (!context.CancellationToken.IsCancellationRequested && lastSyncedTimeStamp.AddDays(1) <= now.Date.AddDays(1));

                await _statusLogger.LogInfoAsync(networkId, "Sync completed", context.CancellationToken);
                _logger.LogInformation("Syncronization Job Completed for Network '{network}'", networkId);
            }
            catch (Exception ex) when (ex.IndicatesTaskCanceled())
            {
                _logger.LogInformation("Synchronization Job cancelled for Network '{network}'", networkId);
                await _statusLogger.LogInfoAsync(networkId, "Sync cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot complete synchronization job for Network {context.JobDetail.Key.Name}.\n{ex.Message}");
                _logger.LogDebug(ex, string.Empty);
                await _statusLogger.LogErrorAsync(networkId, "Sync failed");
            }
        }
    }
}