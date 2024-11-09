using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ICapabilityTester
{
    Task<IEnumerable<ConnectorType>> GetCapabilitiesAsync(CancellationToken stoppingToken = default);
}