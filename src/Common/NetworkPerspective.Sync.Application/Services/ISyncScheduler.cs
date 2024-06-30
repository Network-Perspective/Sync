using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncScheduler
    {
        Task AddOrReplaceAsync(ConnectorInfo connectorInfo, CancellationToken stoppingtoken = default);
        Task EnsureRemovedAsync(ConnectorInfo connectorInfo, CancellationToken stoppingtoken = default);
        Task TriggerNowAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default);
        Task InterruptNowAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default);
        Task ScheduleAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default);
        Task UnscheduleAsync(ConnectorInfo connectorInfod, CancellationToken stoppingToken = default);
        Task<bool> IsScheduledAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default);
        Task<bool> IsRunningAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default);
    }

    /// <summary>
    /// Dummy implementation of <see cref="ISyncScheduler"/> that does nothing
    /// for connectors that don't need scheduling
    /// </summary>
    internal class NoOpSyncScheduler : ISyncScheduler
    {
        public Task AddOrReplaceAsync(ConnectorInfo connectorInfo, CancellationToken stoppingtoken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureRemovedAsync(ConnectorInfo connectorInfo, CancellationToken stoppingtoken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggerNowAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InterruptNowAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task ScheduleAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UnscheduleAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsScheduledAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsRunningAsync(ConnectorInfo connectorInfo, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(false);
        }
    }
}