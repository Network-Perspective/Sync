using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

public interface IGlobalStatusCache
{
    Task<SingleTaskStatus> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default);
    Task SetStatusAsync(Guid connectorId, SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default);
}

internal class GlobalStatusCache : IGlobalStatusCache
{
    private readonly Dictionary<Guid, SingleTaskStatus> _statuses = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<SingleTaskStatus> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);

        try
        {
            return _statuses.TryGetValue(connectorId, out SingleTaskStatus value)
                ? value
                : SingleTaskStatus.Empty;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetStatusAsync(Guid connectorId, SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);

        try
        {
            _statuses[connectorId] = synchronizationTaskStatus;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}