using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Scheduler.Sync;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IStatusService
{
    public Task<Status> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default);
}

internal class StatusService(IWorkerRouter workerRouter, IUnitOfWork unitOfWork, ISyncScheduler scheduler, ILogger<StatusService> logger) : IStatusService
{
    public async Task<Status> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Checking status of connector '{connectorId}'", connectorId);

        var connector = await unitOfWork
            .GetConnectorRepository()
            .GetAsync(connectorId, stoppingToken);

        var logs = await unitOfWork
            .GetStatusLogRepository()
            .GetListAsync(connectorId, stoppingToken);

        var isScheduled = await scheduler.IsScheduledAsync(connectorId, stoppingToken);
        var isConnected = workerRouter.IsConnected(connector.Worker.Name);

        if (!isConnected)
            return Status.Disconnected(isScheduled, logs);

        var connectorStatus = await workerRouter.GetConnectorStatusAsync(connector.Worker.Name, connectorId, connector.NetworkId, connector.Properties, connector.Type);
        return Status.Connected(isScheduled, connectorStatus, logs);
    }
}