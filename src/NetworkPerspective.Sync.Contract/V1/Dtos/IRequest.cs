namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public interface IRequest
{
    public Guid CorrelationId { get; set; }
}
