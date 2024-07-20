using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Utils.Models;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync
{
    internal class RemoteSyncJob : IJob
    {
        private readonly IConnectorsService _connectorsService;
        private readonly IWorkerRouter _router;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly ITokenService _tokenService;
        private readonly IClock _clock;
        private readonly ILogger<RemoteSyncJob> _logger;

        public RemoteSyncJob(IConnectorsService connectorsService, IWorkerRouter router, ISyncHistoryService syncHistoryService, ITokenService tokenService, IClock clock, ILogger<RemoteSyncJob> logger)
        {
            _connectorsService = connectorsService;
            _router = router;
            _syncHistoryService = syncHistoryService;
            _tokenService = tokenService;
            _clock = clock;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var connectorId = Guid.Parse(context.JobDetail.Key.Name);
            var nextSyncStart = await _syncHistoryService.EvaluateSyncStartAsync(connectorId, context.CancellationToken);

            var connector = await _connectorsService.GetAsync(connectorId, context.CancellationToken);
            var accessToken = await _tokenService.GetAsync(connector.Id, context.CancellationToken);

            var syncContext = new SyncContext
            {
                ConnectorId = connectorId,
                ConnectorType = connector.Type,
                NetworkId = connector.NetworkId,
                TimeRange = new TimeRange(nextSyncStart, _clock.UtcNow()),
                AccessToken = accessToken,
                NetworkProperties = connector.Properties
            };

            await _router.StartSyncAsync(connector.Worker.Name, syncContext);

            _logger.LogInformation("Triggered job to order sync connector {connectorId}", connectorId);
        }
    }
}