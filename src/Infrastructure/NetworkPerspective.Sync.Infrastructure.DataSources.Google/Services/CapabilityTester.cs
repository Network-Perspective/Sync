using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal class CapabilityTester(ConnectorType connectorType, ILogger<CapabilityTester> logger) : ICapabilityTester
{
    public Task<IEnumerable<ConnectorType>> GetCapabilitiesAsync(CancellationToken stoppingToken = default)
    {
        logger.LogDebug("We assume that the Google connector can always be added");
        var result = new List<ConnectorType> { connectorType };

        return Task.FromResult(result.AsEnumerable());
    }
}