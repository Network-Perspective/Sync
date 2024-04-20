using NetworkPerspective.Sync.Contract.Dtos;

namespace NetworkPerspective.Sync.Contract;

public interface IConnectorClient
{
    Task<AckResponseDto> StartSyncAsync(StartSyncRequestDto startSyncRequestDto);
}
