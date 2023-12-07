namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}