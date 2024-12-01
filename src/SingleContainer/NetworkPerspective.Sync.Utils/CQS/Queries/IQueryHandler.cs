using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IQueryHandler<in TRequest, TResponse>
    where TRequest : class, IQuery<TResponse>
    where TResponse : class, IResponse
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}
