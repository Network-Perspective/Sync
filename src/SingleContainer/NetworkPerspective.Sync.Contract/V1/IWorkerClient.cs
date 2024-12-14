using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public interface IWorkerClient
{
    Task<AckDto> SyncAsync(SyncRequest startSyncRequestDto);
    Task<AckDto> SetSecretsAsync(SetSecretsRequest setSecretsRequestDto);
    Task<AckDto> RotateSecretsAsync(RotateSecretsRequest rotateSecretsDto);
    Task<ConnectorStatusResponse> GetConnectorStatusAsync(ConnectorStatusRequest getConnectorStatusDto);
    Task<WorkerCapabilitiesResponse> GetWorkerCapabilitiesAsync(WorkerCapabilitiesRequest getWorkerCapabilites);
    Task<InitializeOAuthResponse> InitializeOAuthAsync(InitializeOAuthRequest request);
    Task<AckDto> HandleOAuthCallbackAsync(HandleOAuthCallbackRequest request);
}