using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Mappers;

internal static class WorkerMapper
{
    public static Worker EntityToDomainModel(WorkerEntity entity)
    {
        return new Worker(entity.Id, entity.Version, entity.Name, entity.SecretHash, entity.SecretSalt, entity.IsAuthorized, entity.CreatedAt);
    }

    public static WorkerEntity DomainModelToEntity(Worker worker)
    {
        return new WorkerEntity
        {
            Id = worker.Id,
            Version = worker.Version,
            Name = worker.Name,
            SecretHash = worker.SecretHash,
            SecretSalt = worker.SecretSalt,
            IsAuthorized = worker.IsAuthorized,
            CreatedAt = worker.CreatedAt
        };
    }
}