using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public interface IWorkerClient
{
    Task<AckDto> StartSyncAsync(StartSyncDto startSyncRequestDto);
}