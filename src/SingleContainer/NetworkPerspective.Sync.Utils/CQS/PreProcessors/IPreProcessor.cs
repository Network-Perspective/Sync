using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.PreProcessors;

public interface IPreProcessor
{
    Task PreprocessAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}