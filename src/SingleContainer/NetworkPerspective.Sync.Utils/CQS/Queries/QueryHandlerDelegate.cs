using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public delegate Task<TResponse> QueryHandlerDelegate<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken)
    where TRequest : class, IQuery<TResponse>
    where TResponse : class, IResponse;