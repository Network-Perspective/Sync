using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class AddLogDto : IRequest
{
    public Guid CorrelationId { get; set; }
    public Guid ConnectorId { get; set; }
    public string Message { get; set; }
    public StatusLogLevel Level { get; set; }
}