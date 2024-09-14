namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;

public class EntityNotFoundException<TType> : DbException
{
    public EntityNotFoundException() : base($"Entity of type {typeof(TType)} cannot be found")
    { }
}