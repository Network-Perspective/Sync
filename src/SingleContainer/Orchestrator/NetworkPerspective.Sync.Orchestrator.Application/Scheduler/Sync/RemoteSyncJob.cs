using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Utils.Models;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

[DisallowConcurrentExecution]
internal class RemoteSyncJob(IConnectorsService connectorsService, IWorkerRouter router, ISyncHistoryService syncHistoryService, ITokenService tokenService, IClock clock, ILogger<RemoteSyncJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var connectorId = Guid.Parse(context.JobDetail.Key.Name);

        try
        {
            var nextSyncStart = await syncHistoryService.EvaluateSyncStartAsync(connectorId, context.CancellationToken);

            var connector = await connectorsService.GetAsync(connectorId, context.CancellationToken);
            var accessToken = await tokenService.GetAsync(connector.Id, context.CancellationToken);

            var syncContext = new SyncContext
            {
                ConnectorId = connectorId,
                ConnectorType = connector.Type,
                NetworkId = connector.NetworkId,
                TimeRange = new TimeRange(nextSyncStart, clock.UtcNow()),
                AccessToken = accessToken,
                NetworkProperties = connector.Properties
            };

            await router.StartSyncAsync(connector.Worker.Name, syncContext);

            logger.LogInformation("Triggered job to order sync connector {connectorId}", connectorId);
        }
        catch (ConnectionNotFoundException cnfx)
        {
            logger.LogWarning("Unable to sync connector '{Id}' because hosting worker '{Name}' is not connected", connectorId, cnfx.WorkerName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Synchronization job failed '{jobKey}'", context.JobDetail.Key.Name);
        }
    }
}