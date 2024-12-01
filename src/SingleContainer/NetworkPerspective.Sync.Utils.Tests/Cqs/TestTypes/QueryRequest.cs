using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class QueryRequest : IQuery<Response>
{
    public Guid CorrelationId { get; set; }
}
