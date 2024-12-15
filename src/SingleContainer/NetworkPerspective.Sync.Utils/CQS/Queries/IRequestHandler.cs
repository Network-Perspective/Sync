using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : class, IResponse
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}