namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class SyncCompletedDto : IRequest
{
    public Guid CorrelationId { get; set; }
    public Guid Start { get; set; }
    public Guid End { get; set; }
}
