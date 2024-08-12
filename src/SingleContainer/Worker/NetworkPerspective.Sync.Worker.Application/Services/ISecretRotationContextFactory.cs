using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.SecretRotation;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISecretRotationContextFactory
{
    Task<SecretRotationContext> CreateAsync(Guid connectorId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
}

internal class SecretRotationContextFactory : ISecretRotationContextFactory
{
    public Task<SecretRotationContext> CreateAsync(Guid connectorId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(new SecretRotationContext(connectorId, properties));
    }
}