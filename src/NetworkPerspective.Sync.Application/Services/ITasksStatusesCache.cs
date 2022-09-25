using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ITasksStatusesCache
    {
        Task<SynchronizationTaskStatus> GetStatusAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task SetStatusAsync(Guid networkId, SynchronizationTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default);
    }

    internal class TasksStatusesCache : ITasksStatusesCache
    {
        private readonly IDictionary<Guid, SynchronizationTaskStatus> _statuses = new Dictionary<Guid, SynchronizationTaskStatus>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<SynchronizationTaskStatus> GetStatusAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(stoppingToken);

                if (_statuses.ContainsKey(networkId))
                    return _statuses[networkId];
                else
                    return SynchronizationTaskStatus.Empty;
            }
            catch (Exception)
            {
                _semaphore.Release();
                return SynchronizationTaskStatus.Empty;
            }
        }

        public async Task SetStatusAsync(Guid networkId, SynchronizationTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(stoppingToken);
                _statuses[networkId] = synchronizationTaskStatus;
            }
            catch(Exception)
            {
                _semaphore.Release();
            }
        }
    }
}
