using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ITasksStatusesCache
    {
        Task<SingleTaskStatus> GetStatusAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task SetStatusAsync(Guid networkId, SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default);
    }

    internal class TasksStatusesCache : ITasksStatusesCache
    {
        private readonly IDictionary<Guid, SingleTaskStatus> _statuses = new Dictionary<Guid, SingleTaskStatus>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<SingleTaskStatus> GetStatusAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(stoppingToken);

                if (_statuses.ContainsKey(networkId))
                    return _statuses[networkId];
                else
                    return SingleTaskStatus.Empty;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetStatusAsync(Guid networkId, SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(stoppingToken);
                _statuses[networkId] = synchronizationTaskStatus;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}