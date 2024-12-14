using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class QueryRequest : IRequest<Response>
{
    public string UserFriendlyName { get; set; } = "Test";
    public Guid CorrelationId { get; set; }
}
