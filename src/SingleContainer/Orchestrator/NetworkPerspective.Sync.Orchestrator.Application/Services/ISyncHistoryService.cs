using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface ISyncHistoryService
{
    Task<DateTime> EvaluateSyncStartAsync(Guid connectorId, CancellationToken stoppingToken = default);
    Task SaveLogAsync(SyncHistoryEntry syncHistoryEntry, CancellationToken stoppingToken = default);
    Task OverrideSyncStartAsync(Guid connectorId, DateTime syncStart, CancellationToken stoppingToken = default);
}

internal class SyncHistoryService : ISyncHistoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<SyncHistoryService> _logger;

    public SyncHistoryService(IUnitOfWork unitOfWork, IClock clock, ILogger<SyncHistoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<DateTime> EvaluateSyncStartAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        var lastSyncHistoryEntry = await _unitOfWork
            .GetSyncHistoryRepository()
            .FindLastLogAsync(connectorId, stoppingToken);
        var lastSyncPeriodEnd = lastSyncHistoryEntry?.SyncPeriod.End;

        _logger.LogDebug("Last synchronization of connector '{connectorId}' {lastSync}", connectorId, lastSyncPeriodEnd?.ToString(Consts.DefaultDateTimeFormat) ?? "not found");

        return lastSyncPeriodEnd ?? _clock.UtcNow();
    }

    public async Task SaveLogAsync(SyncHistoryEntry syncHistoryEntry, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Adding new {type} to persistence", typeof(SyncHistoryEntry));

        await _unitOfWork
            .GetSyncHistoryRepository()
            .AddAsync(syncHistoryEntry, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogDebug("Added {type} to persistence", typeof(SyncHistoryEntry));
    }

    public async Task OverrideSyncStartAsync(Guid connectorId, DateTime syncStart, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Overriding sync start to '{start}' for connector '{connectorId}'", syncStart, connectorId);

        var repository = _unitOfWork.GetSyncHistoryRepository();

        var initLogEntry = SyncHistoryEntry.Create(connectorId, _clock.UtcNow(), new TimeRange(syncStart, syncStart));
        await repository.AddAsync(initLogEntry, stoppingToken);

        await _unitOfWork.CommitAsync(stoppingToken);

        _logger.LogDebug("Overriden sync start to '{start}' for connector '{connectorId}'", syncStart, connectorId);
    }
}