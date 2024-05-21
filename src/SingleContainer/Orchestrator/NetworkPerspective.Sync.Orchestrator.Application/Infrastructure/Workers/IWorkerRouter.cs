using System.Threading.Tasks;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;

public interface IWorkerRouter
{
    Task StartSyncAsync(string workerName, SyncContext syncContext);
}
