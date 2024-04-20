using NetworkPerspective.Sync.Contract.Dtos;

namespace NetworkPerspective.Sync.Contract;

public interface IOrchestratorClient
{
    Task<AckResponseDto> RegisterConnectorAsync(RegisterConnectorRequestDto registerConnectorDto);
}
