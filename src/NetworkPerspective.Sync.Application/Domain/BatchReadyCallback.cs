using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Domain
{
    public delegate Task BatchReadyCallback<T>(BatchReadyEventArgs<T> args);
}
