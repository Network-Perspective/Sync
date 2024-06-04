using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncContextFactory
    {
        Task<SyncContext> CreateForConnectorAsync(Guid connectorId, CancellationToken stoppingToken = default);
    }

    internal class SyncContextFactory : ISyncContextFactory
    {
        private readonly ITokenService _tokenService;
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IConnectorService _networkService;
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly IClock _clock;

        public SyncContextFactory(
            ITokenService tokenService,
            INetworkPerspectiveCore networkPerspectiveCore,
            IStatusLoggerFactory statusLoggerFactory,
            ISyncHistoryService syncHistoryService,
            IConnectorService networkService,
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

        public async Task<SyncContext> CreateForConnectorAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var token = await _tokenService.GetAsync(connectorId, stoppingToken);
            var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(token, stoppingToken);
            var network = await _networkService.GetAsync<ConnectorProperties>(connectorId, stoppingToken);
            var lastSyncedTimeStamp = await _syncHistoryService.EvaluateSyncStartAsync(connectorId, stoppingToken);
            var statusLogger = _statusLoggerFactory.CreateForConnector(connectorId);
            var now = _clock.UtcNow();

            var secretRepository = await _secretRepositoryFactory.CreateAsync(connectorId, stoppingToken);
            var hashingService = await _hashingServiceFactory.CreateAsync(secretRepository, stoppingToken);

            var timeRange = new TimeRange(lastSyncedTimeStamp, now);

            return new SyncContext(connectorId, networkConfig, network.Properties, token, timeRange, statusLogger, hashingService);
        }
    }
}