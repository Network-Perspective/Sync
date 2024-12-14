using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class GetWorkerCapabilitiesHandler(ICapabilitiesService capabilitiesService, ILogger<GetWorkerCapabilitiesHandler> logger) : IQueryHandler<GetWorkerCapabilitiesDto, WorkerCapabilitiesDto>
{
    public async Task<WorkerCapabilitiesDto> HandleAsync(GetWorkerCapabilitiesDto dto, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Checking worker capabilities");

        var connectorTypes = await capabilitiesService.GetSupportedConnectorTypesAsync(stoppingToken);
        return new WorkerCapabilitiesDto
        {
            CorrelationId = dto.CorrelationId,
            SupportedConnectorTypes = connectorTypes.Select(x => x.Name)
        };
    }
}
