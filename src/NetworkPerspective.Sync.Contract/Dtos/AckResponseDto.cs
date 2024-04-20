namespace NetworkPerspective.Sync.Contract.Dtos;

public class AckResponseDto : IResponse
{
    public Guid CorrelationId { get; set; }
}
