using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}