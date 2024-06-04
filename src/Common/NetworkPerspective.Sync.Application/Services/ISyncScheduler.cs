using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncScheduler
    {
        Task AddOrReplaceAsync(Guid connectorId, CancellationToken stoppingtoken = default);
        Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingtoken = default);
        Task TriggerNowAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task InterruptNowAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task ScheduleAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task UnscheduleAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task<bool> IsScheduledAsync(Guid connectorId, CancellationToken stoppingToken = default);
        Task<bool> IsRunningAsync(Guid connectorId, CancellationToken stoppingToken = default);
    }

    /// <summary>
    /// Dummy implementation of <see cref="ISyncScheduler"/> that does nothing
    /// for connectors that don't need scheduling
    /// </summary>
    public class DummySyncScheduler : ISyncScheduler
    {
        public Task AddOrReplaceAsync(Guid connectorId, CancellationToken stoppingtoken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingtoken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggerNowAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InterruptNowAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task ScheduleAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UnscheduleAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsScheduledAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsRunningAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(false);
        }
    }
}