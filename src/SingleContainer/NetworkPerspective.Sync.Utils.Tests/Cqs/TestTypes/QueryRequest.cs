using System;

using NetworkPerspective.Sync.Utils.CQS;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class QueryRequest : IRequest<Response>
{
    public Guid CorrelationId { get; set; }
}
