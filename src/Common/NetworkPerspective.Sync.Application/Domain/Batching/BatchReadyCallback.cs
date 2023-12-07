using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Domain.Batching
{
    public delegate Task BatchReadyCallback<T>(BatchReadyEventArgs<T> args);
}