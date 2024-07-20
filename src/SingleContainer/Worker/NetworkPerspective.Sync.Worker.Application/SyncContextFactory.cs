using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Worker.Application;

public interface ISyncContextFactory
{
    Task<SyncContext> CreateAsync(Guid connectorId, string type, IDictionary<string, string> properties, TimeRange timeRange, SecureString accessToken, CancellationToken stoppingToken = default);
}

internal class SyncContextFactory : ISyncContextFactory
{
    private readonly ISecretRepositoryFactory _secretRepositoryFactory;
    private readonly IHashingServiceFactory _hashingServiceFactory;
    private readonly INetworkPerspectiveCore _networkPerspectiveCore;

    public SyncContextFactory(ISecretRepositoryFactory secretRepositoryFactory, IHashingServiceFactory hashingServiceFactory, INetworkPerspectiveCore networkPerspectiveCore)
    {
        _secretRepositoryFactory = secretRepositoryFactory;
        _hashingServiceFactory = hashingServiceFactory;
        _networkPerspectiveCore = networkPerspectiveCore;
    }

    public async Task<SyncContext> CreateAsync(Guid connectorId, string type, IDictionary<string, string> properties, TimeRange timeRange, SecureString accessToken, CancellationToken stoppingToken = default)
    {
        var connectorProperties = ConnectorProperties.Create<ConnectorProperties>(properties);
        var secretRepository = _secretRepositoryFactory.Create(connectorProperties.ExternalKeyVaultUri);

        var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(accessToken, stoppingToken);
        var hashingService = await _hashingServiceFactory.CreateAsync(secretRepository, stoppingToken);

        return new SyncContext(connectorId, type, networkConfig, properties, accessToken, timeRange, hashingService);
    }
}