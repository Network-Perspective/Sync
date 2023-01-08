using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Core;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncContextFactory
    {
        Task<SyncContext> CreateForNetworkAsync(Guid networkId, CancellationToken stoppingToken = default);
    }

    internal class SyncContextFactory : ISyncContextFactory
    {
        private readonly ITokenService _tokenService;
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IClock _clock;

        public SyncContextFactory(ITokenService tokenService, INetworkPerspectiveCore networkPerspectiveCore, IStatusLoggerFactory statusLoggerFactory, ISyncHistoryService syncHistoryService, IClock clock)
        {
            _tokenService = tokenService;
            _networkPerspectiveCore = networkPerspectiveCore;
            _statusLoggerFactory = statusLoggerFactory;
            _syncHistoryService = syncHistoryService;
            _clock = clock;
        }

        public async Task<SyncContext> CreateForNetworkAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var token = await _tokenService.GetAsync(networkId, stoppingToken);
            var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(token, stoppingToken);
            var lastSyncedTimeStamp = await _syncHistoryService.EvaluateSyncStartAsync(networkId, stoppingToken);
            var statusLogger = _statusLoggerFactory.CreateForNetwork(networkId);
            var now = _clock.UtcNow();

            var timeRange = new TimeRange(lastSyncedTimeStamp, now);

            return new SyncContext(networkId, networkConfig, token, timeRange, statusLogger);
        }
    }

}