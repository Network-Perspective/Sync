using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class SynchronizationTaskStatusMapper
{
    public static SynchronizationTaskStatusDto DomainTaskStatusToDto(SingleTaskStatus status)
    {
        if (status == null)
            return null;

        return new SynchronizationTaskStatusDto
        {
            Caption = status.Caption,
            Description = status.Description,
            CompletionRate = status.CompletionRate
        };
    }
}