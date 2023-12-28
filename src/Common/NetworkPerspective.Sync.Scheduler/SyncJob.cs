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
        private readonly ISyncServiceFactory _syncServiceFactory;
        private readonly ISyncContextProvider _syncContextProvider;
        private readonly ILogger<SyncJob> _logger;

        public SyncJob(
            ISyncServiceFactory syncServiceFactory,
            ISyncContextProvider syncContextProvider,
            ILogger<SyncJob> logger)
        {
            _syncServiceFactory = syncServiceFactory;
            _syncContextProvider = syncContextProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Triggered synchronization job for network '{network}'", _syncContextProvider.Context.NetworkId);
                var syncService = await _syncServiceFactory.CreateAsync(_syncContextProvider.Context.NetworkId, context.CancellationToken);
                await syncService.SyncAsync(_syncContextProvider.Context, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Synchronization job failed '{jobKey}'", context.JobDetail.Key.Name);
            }
        }
    }
}