using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;

public interface IConnectorPropertyRepository
{
    Task SetAsync(Guid connectorId, IDictionary<string, string> properties, CancellationToken stoppingToken = default);
}