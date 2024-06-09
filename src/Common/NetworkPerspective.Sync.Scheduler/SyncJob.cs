using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Application.Scheduler
{
    [DisallowConcurrentExecution]
    internal class SyncJob : IJob
    {
        private readonly ISyncService _syncService;
        private readonly ISyncContextProvider _syncContextProvider;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IClock _clock;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncService syncService,
            ISyncContextProvider syncContextProvider,
            ISyncHistoryService syncHistoryService,
            IClock clock,
            ILogger<SyncJob> logger)
        {
            _syncService = syncService;
            _syncContextProvider = syncContextProvider;
            _syncHistoryService = syncHistoryService;
            _clock = clock;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var syncContext = await _syncContextProvider.GetAsync(context.CancellationToken);
                _logger.LogInformation("Triggered synchronization job for network '{network}'", syncContext.ConnectorId);
                var syncResult = await _syncService.SyncAsync(syncContext, context.CancellationToken);

                if (syncResult != SyncResult.Empty)
                {
                    var syncHistoryEntry = SyncHistoryEntry.CreateWithResult(syncContext.ConnectorId, _clock.UtcNow(), syncContext.TimeRange, syncResult);
                    await _syncHistoryService.SaveLogAsync(syncHistoryEntry, context.CancellationToken);
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Synchronization job failed '{jobKey}'", context.JobDetail.Key.Name);
            }
        }
    }
}