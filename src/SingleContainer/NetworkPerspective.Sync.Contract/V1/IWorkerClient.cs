using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public interface IWorkerClient
{
    Task<AckDto> PullSyncAsync(StartSyncDto startSyncRequestDto);
    Task<SyncCompletedDto> PushSyncAsync(SyncRequestDto syncRequestDto);
    Task<AckDto> SetSecretsAsync(SetSecretsDto setSecretsRequestDto);
    Task<AckDto> RotateSecretsAsync(RotateSecretsDto rotateSecretsDto);
}