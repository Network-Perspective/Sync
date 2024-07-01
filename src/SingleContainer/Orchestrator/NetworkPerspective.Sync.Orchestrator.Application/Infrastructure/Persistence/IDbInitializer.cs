using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}