using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Scheduler
{
    internal class SyncContextProvider : ISyncContextProvider, ISyncContextInitializer, IDisposable
    {
        private readonly ISyncContextFactory _syncContextFactory;

        public SyncContext Context { get; private set; } = null;

        public SyncContextProvider(ISyncContextFactory syncContextFactory)
        {
            _syncContextFactory = syncContextFactory;
        }

        public async Task InitializeAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            Context = await _syncContextFactory.CreateForNetworkAsync(networkId, stoppingToken);
        }


        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}