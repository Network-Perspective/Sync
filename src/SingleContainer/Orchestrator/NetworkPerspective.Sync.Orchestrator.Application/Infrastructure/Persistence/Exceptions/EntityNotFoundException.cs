namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;

public class EntityNotFoundException<TType> : DbException
{
    public EntityNotFoundException(string id) : base($"Entity of type {typeof(TType)} and identifier \"{id}\" cannot be found")
    { }
}