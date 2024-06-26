using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.SecretRotation;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application;

internal class SecretRotationContextFactory : ISecretRotationContextFactory
{
    public Task<SecretRotationContext> CreateAsync(Guid connectorId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(new SecretRotationContext(connectorId, properties));
    }
}
