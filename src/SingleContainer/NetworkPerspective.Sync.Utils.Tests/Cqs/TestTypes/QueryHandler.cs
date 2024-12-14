using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class QueryHandler : IRequestHandler<QueryRequest, Response>
{
    public Task<Response> HandleAsync(QueryRequest request, CancellationToken stoppingToken = default)
        => Task.FromResult(new Response());
}
