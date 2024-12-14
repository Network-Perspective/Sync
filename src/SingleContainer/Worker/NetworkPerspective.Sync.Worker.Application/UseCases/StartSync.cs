using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases;

internal class StartSyncHandler(ISyncContextFactory syncContextFactory, ISyncContextAccessor syncContextAccessor, ISyncService syncService, ILogger<StartSyncHandler> logger) : IQueryHandler<StartSyncDto, SyncCompletedDto>
{
    public async Task<SyncCompletedDto> HandleAsync(StartSyncDto dto, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Syncing started for connector '{connectorId}'", dto.Connector.Id);

        var timeRange = new TimeRange(dto.Start, dto.End);
        var accessToken = dto.AccessToken.ToSecureString();

        var syncContext = await syncContextFactory.CreateAsync(dto.Connector.Id, dto.Connector.Type, dto.Connector.Properties, timeRange, accessToken, stoppingToken);

        if (dto.Employees is not null)
            syncContext.Set(dto.Employees);

        syncContextAccessor.SyncContext = syncContext;

        var result = await syncService.SyncAsync(syncContext, stoppingToken);
        logger.LogInformation("Sync for connector '{connectorId}' completed", dto.Connector.Id);

        return new SyncCompletedDto
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
}
