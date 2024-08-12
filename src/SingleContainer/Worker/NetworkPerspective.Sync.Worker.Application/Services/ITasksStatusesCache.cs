using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ITasksStatusesCache
{
    Task<SingleTaskStatus> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default);
    Task SetStatusAsync(Guid connectorId, SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default);
}

internal class TasksStatusesCache : ITasksStatusesCache
{
    private readonly IDictionary<Guid, SingleTaskStatus> _statuses = new Dictionary<Guid, SingleTaskStatus>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<SingleTaskStatus> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(stoppingToken);

            if (_statuses.ContainsKey(connectorId))
                return _statuses[connectorId];
            else
                return SingleTaskStatus.Empty;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetStatusAsync(Guid connectorId, SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(stoppingToken);
            _statuses[connectorId] = synchronizationTaskStatus;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}