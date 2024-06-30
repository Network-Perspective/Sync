using System.Linq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class StatusMapper
{
    public static StatusDto DomainStatusToDto(Status status)
    {
        return new StatusDto
        {
            Authorized = status.Authorized,
            Scheduled = status.Scheduled,
            Running = status.Running,
            CurrentTask = SynchronizationTaskStatusMapper.DomainTaskStatusToDto(status.CurrentTask),
            Logs = status.Logs.Select(StatusLogMapper.DomainStatusLogToDto)
        };
    }
}