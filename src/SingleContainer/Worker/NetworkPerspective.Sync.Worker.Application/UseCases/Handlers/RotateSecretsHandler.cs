using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Services;


namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class RotateSecretsHandler(ISecretRotationService rotationService, ILogger<RotateSecretsHandler> logger) : IQueryHandler<RotateSecretsDto, AckDto>
{
    public async Task<AckDto> HandleAsync(RotateSecretsDto dto, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Rotating secrets for connector '{connectorId}' of type '{type}'", dto.Connector.Id, dto.Connector.Type);
        await rotationService.ExecuteAsync(stoppingToken);
        logger.LogInformation("Secrets has been rotated");

        return new AckDto { CorrelationId = dto.Connector.Id };
    }
}
