using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISyncContextFactory
{
    Task<SyncContext> CreateAsync(Guid connectorId, string type, IDictionary<string, string> properties, TimeRange timeRange, SecureString accessToken, CancellationToken stoppingToken = default);
}

internal class SyncContextFactory(INetworkPerspectiveCore networkPerspectiveCore) : ISyncContextFactory
{
    public async Task<SyncContext> CreateAsync(Guid connectorId, string type, IDictionary<string, string> properties, TimeRange timeRange, SecureString accessToken, CancellationToken stoppingToken = default)
    {
        var networkConfig = await networkPerspectiveCore.GetNetworkConfigAsync(accessToken, stoppingToken);

        return new SyncContext(connectorId, type, networkConfig, properties, accessToken, timeRange);
    }
}