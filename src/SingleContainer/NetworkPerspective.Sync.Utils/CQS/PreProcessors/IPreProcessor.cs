using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.PreProcessors;

public interface IPreProcessor
{
    Task PreprocessAsync<TCommand>(TCommand command, IServiceScope scope, CancellationToken cancellationToken)
        where TCommand : class, ICommand;

    Task PreprocessAsync<TQuery, TResponse>(TQuery request, IServiceScope scope, CancellationToken cancellationToken)
        where TQuery : class, IQuery<TResponse>
        where TResponse : class, IResponse;
}
