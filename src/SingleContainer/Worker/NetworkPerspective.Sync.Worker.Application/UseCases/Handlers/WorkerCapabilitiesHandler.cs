using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class WorkerCapabilitiesHandler(ICapabilitiesService capabilitiesService, ILogger<WorkerCapabilitiesHandler> logger) : IRequestHandler<WorkerCapabilitiesRequest, WorkerCapabilitiesResponse>
{
    public async Task<WorkerCapabilitiesResponse> HandleAsync(WorkerCapabilitiesRequest dto, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Checking worker capabilities");

        var connectorTypes = await capabilitiesService.GetSupportedConnectorTypesAsync(stoppingToken);
        return new WorkerCapabilitiesResponse
        {
            CorrelationId = dto.CorrelationId,
            SupportedConnectorTypes = connectorTypes.Select(x => x.Name)
        };
    }
}
