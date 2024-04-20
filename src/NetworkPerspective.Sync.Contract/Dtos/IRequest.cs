namespace NetworkPerspective.Sync.Contract.Dtos;

public interface IRequest
{
    public Guid CorrelationId { get; set; }
}
