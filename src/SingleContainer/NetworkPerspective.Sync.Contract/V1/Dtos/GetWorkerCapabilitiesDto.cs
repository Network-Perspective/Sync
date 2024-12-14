using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class GetWorkerCapabilitiesDto : IQuery<WorkerCapabilitiesDto>
{
    public string UserFriendlyName { get; set; } = "Get Worker Capabilities";
    public Guid CorrelationId { get; set; }
}