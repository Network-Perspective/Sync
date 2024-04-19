using System.Threading.Tasks;

namespace NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

public interface IQueryHandler<in TQueryArgs, TQueryResult>
    where TQueryArgs : IQueryArgs
    where TQueryResult : IQueryResult
{
    Task<TQueryResult> HandleAsync(TQueryArgs args);
}
