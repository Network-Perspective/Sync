using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class AddLogDto : IQuery<AckDto>
{
    public string UserFriendlyName { get; set; } = "Add Log";
    public Guid CorrelationId { get; set; }
    public Guid ConnectorId { get; set; }
    public string Message { get; set; }
    public StatusLogLevel Level { get; set; }
}