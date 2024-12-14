using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.CQS.Queries;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : class, IQuery<TResponse>
    where TResponse : class, IResponse
{
    Task<TResponse> HandleAsync(TQuery dto, CancellationToken stoppingToken = default);
}
