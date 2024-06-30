using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class StatusLogMapper
{
    public static StatusLogDto DomainStatusLogToDto(StatusLog status)
    {
        return new StatusLogDto
        {
            TimeStamp = status.TimeStamp,
            Message = status.Message,
            Level = status.Level
        };
    }
}