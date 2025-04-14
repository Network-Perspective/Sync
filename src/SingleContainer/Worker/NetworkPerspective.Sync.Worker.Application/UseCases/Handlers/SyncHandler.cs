using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class SyncHandler(ISyncContextFactory syncContextFactory, ISyncContextAccessor syncContextAccessor, ISyncService syncService, IGlobalStatusCache tasksStatusesCache, ILogger<SyncHandler> logger)
    : IRequestHandler<SyncRequest, SyncResponse>
{
    private static readonly SemaphoreSlim Semaphore = new(1);

    public async Task<SyncResponse> HandleAsync(SyncRequest dto, CancellationToken stoppingToken = default)
    {
        try
        {
            logger.LogInformation("Syncing started for connector '{connectorId}'", dto.Connector.Id);

            await LockForConnectorAsync(dto.Connector.Id, stoppingToken);

            var timeRange = new TimeRange(dto.Start, dto.End);
            var accessToken = dto.AccessToken.ToSecureString();

            var syncContext = await syncContextFactory.CreateAsync(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties, timeRange, accessToken, stoppingToken);

            if (dto.Employees is not null)
                syncContext.Set(dto.Employees);

            syncContextAccessor.SyncContext = syncContext;

            var result = await syncService.SyncAsync(syncContext, stoppingToken);
            logger.LogInformation("Sync for connector '{connectorId}' completed", dto.Connector.Id);

            return new SyncResponse
            {
                CorrelationId = dto.CorrelationId,
                ConnectorId = dto.Connector.Id,
                Start = dto.Start,
                End = dto.End,
                TasksCount = result.TasksCount,
                FailedTasksCount = result.FailedTasksCount,
                SuccessRate = result.SuccessRate,
                TotalInteractionsCount = result.TotalInteractionsCount
            };
        }
        finally
        {
            await tasksStatusesCache.SetStatusAsync(dto.Connector.Id, SingleTaskStatus.Empty, stoppingToken);
        }
    }

    private async Task LockForConnectorAsync(Guid connectorId, CancellationToken stoppingToken)
    {
        await Semaphore.WaitAsync(stoppingToken);

        try
        {
            if (await tasksStatusesCache.GetStatusAsync(connectorId, stoppingToken) != SingleTaskStatus.Empty)
                throw new SyncAlreadyInProgressException(connectorId);

            var status = SingleTaskStatus.New("Initializing synchronization", "The synchronization is starting", 0);
            await tasksStatusesCache.SetStatusAsync(connectorId, status, stoppingToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}