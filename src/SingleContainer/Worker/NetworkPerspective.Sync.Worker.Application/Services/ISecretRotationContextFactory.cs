using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.SecretRotation;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISecretRotationContextFactory
{
    SecretRotationContext Create(Guid connectorId, IDictionary<string, string> properties);
}

internal class SecretRotationContextFactory : ISecretRotationContextFactory
{
    public SecretRotationContext Create(Guid connectorId, IDictionary<string, string> properties)
    {
        return new SecretRotationContext(connectorId, properties);
    }
}