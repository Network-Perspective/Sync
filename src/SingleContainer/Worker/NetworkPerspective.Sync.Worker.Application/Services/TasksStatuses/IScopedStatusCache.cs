using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

public interface IScopedStatusCache
{
    Task<SingleTaskStatus> GetStatusAsync(CancellationToken stoppingToken = default);
    Task SetStatusAsync(SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default);
}

internal class ScopedStatusCache(IConnectorContextAccessor contextAccessor, IGlobalStatusCache tasksCache) : IScopedStatusCache
{
    public Task<SingleTaskStatus> GetStatusAsync(CancellationToken stoppingToken = default)
        => tasksCache.GetStatusAsync(contextAccessor.Context.ConnectorId, stoppingToken);

    public Task SetStatusAsync(SingleTaskStatus synchronizationTaskStatus, CancellationToken stoppingToken = default)
        => tasksCache.SetStatusAsync(contextAccessor.Context.ConnectorId, synchronizationTaskStatus, stoppingToken);
}