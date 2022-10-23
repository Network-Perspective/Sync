using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncScheduler
    {
        Task AddOrReplaceAsync(Guid networkId, CancellationToken stoppingtoken = default);
        Task EnsureRemovedAsync(Guid networkId, CancellationToken stoppingtoken = default);
        Task TriggerNowAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task InterruptNowAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task ScheduleAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task UnscheduleAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task<bool> IsScheduledAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task<bool> IsRunningAsync(Guid networkId, CancellationToken stoppingToken = default);
    }
}