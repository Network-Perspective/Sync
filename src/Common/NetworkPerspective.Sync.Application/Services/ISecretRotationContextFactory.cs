using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.SecretRotation;

namespace NetworkPerspective.Sync.Application.Services;

public interface ISecretRotationContextFactory
{
    Task<SecretRotationContext> CreateAsync(Guid connectorId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
}
