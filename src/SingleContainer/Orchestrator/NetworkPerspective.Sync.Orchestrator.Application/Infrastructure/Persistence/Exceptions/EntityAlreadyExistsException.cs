namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;

public class EntityAlreadyExistsException : DbException
{
    public EntityAlreadyExistsException(string message)
        : base(message)
    { }
}
