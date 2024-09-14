using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISyncContextFactory
{
    Task<SyncContext> CreateAsync(Guid connectorId, string type, IDictionary<string, string> properties, TimeRange timeRange, SecureString accessToken, CancellationToken stoppingToken = default);
}

internal class SyncContextFactory : ISyncContextFactory
{
    private readonly IVault _secretRepository;
    private readonly IHashingServiceFactory _hashingServiceFactory;
    private readonly INetworkPerspectiveCore _networkPerspectiveCore;

    public SyncContextFactory(IVault secretRepository, IHashingServiceFactory hashingServiceFactory, INetworkPerspectiveCore networkPerspectiveCore)
    {
        _secretRepository = secretRepository;
        _hashingServiceFactory = hashingServiceFactory;
        _networkPerspectiveCore = networkPerspectiveCore;
    }

    public async Task<SyncContext> CreateAsync(Guid connectorId, string type, IDictionary<string, string> properties, TimeRange timeRange, SecureString accessToken, CancellationToken stoppingToken = default)
    {
        var connectorProperties = ConnectorProperties.Create<ConnectorProperties>(properties);

        var networkConfig = await _networkPerspectiveCore.GetNetworkConfigAsync(accessToken, stoppingToken);
        var hashingService = await _hashingServiceFactory.CreateAsync(_secretRepository, stoppingToken);

        return new SyncContext(connectorId, type, networkConfig, properties, accessToken, timeRange, hashingService);
    }
}