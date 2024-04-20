using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public interface IConnectorClient
{
    Task<AckDto> StartSyncAsync(StartSyncDto startSyncRequestDto);
}
