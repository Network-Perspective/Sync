using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class WorkerCapabilitiesRequest : IRequest<WorkerCapabilitiesResponse>
{
    public string UserFriendlyName { get; set; } = "Get Worker Capabilities";
    public Guid CorrelationId { get; set; }
}