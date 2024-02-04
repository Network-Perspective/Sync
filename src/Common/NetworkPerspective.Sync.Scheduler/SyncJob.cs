using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Application.Scheduler
{
    [DisallowConcurrentExecution]
    internal class SyncJob : IJob
    {
        private readonly ISyncService _syncService;
        private readonly ISyncContextProvider _syncContextProvider;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncService syncService,
            ISyncContextProvider syncContextProvider,
            ILogger<SyncJob> logger)
        {
            _syncService = syncService;
            _syncContextProvider = syncContextProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var syncContext = await _syncContextProvider.GetAsync(context.CancellationToken);
                _logger.LogInformation("Triggered synchronization job for network '{network}'", syncContext.NetworkId);
                await _syncService.SyncAsync(syncContext, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Synchronization job failed '{jobKey}'", context.JobDetail.Key.Name);
            }
        }
    }
}