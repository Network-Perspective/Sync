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
        private readonly ISyncContextFactory _syncContextFactory;
        private readonly ISyncServiceFactory _syncServiceFactory;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncContextFactory syncContextFactory,
            ISyncServiceFactory syncServiceFactory,
            ILogger<SyncJob> logger)
        {
            _syncContextFactory = syncContextFactory;
            _syncServiceFactory = syncServiceFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var networkId = GetNetworkId(context);
                _logger.LogInformation("Triggered synchronization job for network '{network}'", networkId);
                using var syncContext = await _syncContextFactory.CreateForNetworkAsync(networkId, context.CancellationToken);
                var syncService = await _syncServiceFactory.CreateAsync(syncContext.NetworkId, context.CancellationToken);
                await syncService.SyncAsync(syncContext, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Synchronization job failed '{jobKey}'", context.JobDetail.Key.Name);
            }

        }

        private static Guid GetNetworkId(IJobExecutionContext context)
            => Guid.TryParse(context.JobDetail.Key.Name, out Guid networkId) ? networkId : Guid.Empty;
    }
}