using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

internal static class WorkerMapper
{
    public static Worker EntityToDomainModel(WorkerEntity entity)
    {
        return new Worker(entity.Id, entity.Name, entity.SecretHash, entity.SecretSalt, entity.IsAuthorized, entity.CreatedAt);
    }

    public static WorkerEntity DomainModelToEntity(Worker worker)
    {
        return new WorkerEntity
        {
            Id = worker.Id,
            Name = worker.Name,
            SecretHash = worker.SecretHash,
            SecretSalt = worker.SecretSalt,
            IsAuthorized = worker.IsAuthorized,
            CreatedAt = worker.CreatedAt
        };
    }
}