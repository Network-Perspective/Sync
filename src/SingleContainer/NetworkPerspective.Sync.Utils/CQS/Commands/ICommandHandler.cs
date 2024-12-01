using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.CQS.Commands;

public interface ICommandHandler<in TRequest>
    where TRequest : class, ICommand
{
    Task HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}
