using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public interface IWorkerClient
{
    Task<AckDto> SyncAsync(StartSyncDto startSyncRequestDto);
    Task<AckDto> SetSecretsAsync(SetSecretsDto setSecretsRequestDto);
    Task<AckDto> RotateSecretsAsync(RotateSecretsDto rotateSecretsDto);
    Task<ConnectorStatusDto> GetConnectorStatusAsync(GetConnectorStatusDto getConnectorStatusDto);
    Task<WorkerCapabilitiesDto> GetWorkerCapabilitiesAsync(GetWorkerCapabilitiesDto getWorkerCapabilites);
    Task<InitializeOAuthResponse> InitializeOAuthAsync(InitializeOAuthRequest request);
    Task<AckDto> HandleOAuthCallbackAsync(HandleOAuthCallbackRequest request);
}