namespace NetworkPerspective.Sync.Contract.Dtos;

public class StartSyncRequestDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
