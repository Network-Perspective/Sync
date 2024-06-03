using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Utils.Models;

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
        private readonly INetworkService _networkService;
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly IClock _clock;

        public SyncContextFactory(
            ITokenService tokenService,
            INetworkPerspectiveCore networkPerspectiveCore,
            IStatusLoggerFactory statusLoggerFactory,
            ISyncHistoryService syncHistoryService,
            INetworkService networkService,
            IHashingServiceFactory hashingServiceFactory,
            ISecretRepositoryFactory secretRepositoryFactory,
            IClock clock)
        {
            _tokenService = tokenService;
            _networkPerspectiveCore = networkPerspectiveCore;
            _statusLoggerFactory = statusLoggerFactory;
            _syncHistoryService = syncHistoryService;
            _networkService = networkService;
            _hashingServiceFactory = hashingServiceFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
            _clock = clock;
        }

        public async Task<SyncContext> CreateForNetworkAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var token = await _tokenService.GetAsync(networkId, stoppingToken);
            var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(token, stoppingToken);
            var network = await _networkService.GetAsync<NetworkProperties>(networkId, stoppingToken);
            var lastSyncedTimeStamp = await _syncHistoryService.EvaluateSyncStartAsync(networkId, stoppingToken);
            var statusLogger = _statusLoggerFactory.CreateForNetwork(networkId);
            var now = _clock.UtcNow();

            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
            var hashingService = await _hashingServiceFactory.CreateAsync(secretRepository, stoppingToken);

            var timeRange = new TimeRange(lastSyncedTimeStamp, now);

            return new SyncContext(networkId, networkConfig, network.Properties, token, timeRange, statusLogger, hashingService);
        }
    }
}