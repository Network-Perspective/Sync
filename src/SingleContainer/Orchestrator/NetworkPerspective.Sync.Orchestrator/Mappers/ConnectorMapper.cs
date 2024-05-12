using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class ConnectorMapper
{
    public static ConnectorDto ToDto(Connector connector)
    {
        return new ConnectorDto
        {
            Id = connector.Id,
            WorkerId = connector.Worker.Id,
            Type = connector.Type
        };
    }
}