using NetworkPerspective.Sync.Contract.Dtos;

namespace NetworkPerspective.Sync.Contract;

public class RegisterConnectorRequestDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
}
