using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public interface IOrchestratorClient
{
    Task<PongDto> PingAsync(PingDto syncCompleted);
    Task<AckDto> SyncCompletedAsync(SyncResponse syncCompleted);
    Task<AckDto> AddLogAsync(AddLogDto addLog);
}