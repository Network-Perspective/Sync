using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.CQS.Commands;

public delegate Task CommandHandlerDelegate<TRequest>(TRequest request, CancellationToken stoppingToken)
    where TRequest : class, ICommand;
