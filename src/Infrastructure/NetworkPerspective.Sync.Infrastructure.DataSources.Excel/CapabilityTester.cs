using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

internal class CapabilityTester(ConnectorType connectorType, ILogger<CapabilityTester> logger) : ICapabilityTester
{
    public Task<IEnumerable<ConnectorType>> GetCapabilitiesAsync(CancellationToken stoppingToken = default)
    {
        logger.LogDebug("We assume that the Excel connector can always be added");
        var result = new List<ConnectorType> { connectorType };

        return Task.FromResult(result.AsEnumerable());
    }
}