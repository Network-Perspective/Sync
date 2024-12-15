using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ICapabilitiesService
{
    Task<IEnumerable<ConnectorType>> GetSupportedConnectorTypesAsync(CancellationToken stoppingToken);
}

internal class CapabilitiesService(IEnumerable<ICapabilityTester> testers) : ICapabilitiesService
{
    public async Task<IEnumerable<ConnectorType>> GetSupportedConnectorTypesAsync(CancellationToken stoppingToken)
    {
        var tasks = testers.Select(x => x.GetCapabilitiesAsync(stoppingToken));
        var result = await Task.WhenAll(tasks);
        return result.SelectMany(x => x);
    }
}