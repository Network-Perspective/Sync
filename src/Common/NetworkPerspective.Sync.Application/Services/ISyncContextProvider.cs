using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncContextProvider
    {
        Task<SyncContext> GetAsync(CancellationToken stoppingToken = default);
    }

    internal class SyncContextProvider : ISyncContextProvider, IDisposable
    {
        private readonly ISyncContextFactory _syncContextFactory;
        private readonly IConnectorInfoProvider _connectorInfoProvider;

        private SyncContext _syncContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public async Task<SyncContext> GetAsync(CancellationToken stoppingToken = default)
        {
            await _semaphore.WaitAsync(stoppingToken);

            try
            {
                if (_syncContext == null)
                {
                    var connectorInfo = _connectorInfoProvider.Get();
                    _syncContext = await _syncContextFactory.CreateForConnectorAsync(connectorInfo.Id, stoppingToken);
                }

                return _syncContext;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public SyncContextProvider(ISyncContextFactory syncContextFactory, IConnectorInfoProvider connectorInfoProvider)
        {
            _syncContextFactory = syncContextFactory;
            _connectorInfoProvider = connectorInfoProvider;
        }


        public void Dispose()
        {
            _syncContext?.Dispose();
        }
    }
}