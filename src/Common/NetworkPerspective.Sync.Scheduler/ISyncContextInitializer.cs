using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Scheduler
{
    internal interface ISyncContextInitializer
    {
        Task InitializeAsync(Guid networkId, CancellationToken stoppingToken = default);
    }
}