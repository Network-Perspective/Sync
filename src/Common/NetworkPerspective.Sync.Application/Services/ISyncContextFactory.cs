using System;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IConnectorService _connectorService;
        private readonly IHashingServiceFactory _hashingServiceFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly IClock _clock;

        public SyncContextFactory(
            ITokenService tokenService,
            INetworkPerspectiveCore networkPerspectiveCore,
            ISyncHistoryService syncHistoryService,
            IConnectorService connectorService,
            IHashingServiceFactory hashingServiceFactory,
            ISecretRepositoryFactory secretRepositoryFactory,
            IClock clock)
        {
            _tokenService = tokenService;
            _networkPerspectiveCore = networkPerspectiveCore;
            _syncHistoryService = syncHistoryService;
            _connectorService = connectorService;
            _hashingServiceFactory = hashingServiceFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
            _clock = clock;
        }

        public async Task<SyncContext> CreateForConnectorAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            var token = await _tokenService.GetAsync(connectorId, stoppingToken);
            var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(token, stoppingToken);
            var properties = await _connectorService.GetProperties(connectorId, stoppingToken);
            var lastSyncedTimeStamp = await _syncHistoryService.EvaluateSyncStartAsync(connectorId, stoppingToken);
            var now = _clock.UtcNow();


            var connectorProperties = ConnectorProperties.Create<ConnectorProperties>(properties);
            var secretRepository = _secretRepositoryFactory.Create(connectorProperties.ExternalKeyVaultUri);
            var hashingService = await _hashingServiceFactory.CreateAsync(secretRepository, stoppingToken);

            var timeRange = new TimeRange(lastSyncedTimeStamp, now);

            return new SyncContext(connectorId, string.Empty, networkConfig, properties, token, timeRange, hashingService);
        }
    }
}