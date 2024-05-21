using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler
{
    internal class RemoteSyncJob : IJob
    {
        private readonly IConnectorsService _connectorsService;
        private readonly IWorkerRouter _router;
        private readonly IClock _clock;
        private readonly ILogger<RemoteSyncJob> _logger;

        public RemoteSyncJob(IConnectorsService connectorsService, IWorkerRouter router, IClock clock, ILogger<RemoteSyncJob> logger)
        {
            _connectorsService = connectorsService;
            _router = router;
            _clock = clock;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var connectorId = Guid.Parse(context.JobDetail.Key.Name);

            var connector = await _connectorsService.GetAsync(connectorId, context.CancellationToken);
            var syncContext = new SyncContext
            { 
                Start = _clock.UtcNow().AddDays(-10),
                End = _clock.UtcNow()
            };

            await _router.StartSyncAsync(connector.Worker.Name, syncContext);

            _logger.LogInformation("Triggered job to order sync connector {connectorId}", connectorId);
        }
    }
}