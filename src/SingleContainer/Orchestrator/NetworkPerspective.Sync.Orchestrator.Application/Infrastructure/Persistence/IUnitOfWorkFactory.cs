namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}