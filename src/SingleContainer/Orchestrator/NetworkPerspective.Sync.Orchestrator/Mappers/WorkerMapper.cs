using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class WorkerMapper
{
    public static WorkerDto ToDto(Worker worker)
    {
        return new WorkerDto
        {
            Id = worker.Id,
            Name = worker.Name,
            IsAuthorized = worker.IsAuthorized
        };
    }
}