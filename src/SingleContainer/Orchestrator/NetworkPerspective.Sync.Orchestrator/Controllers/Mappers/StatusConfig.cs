using Mapster;

using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Mappers;

public class StatusConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Status, StatusDto>()
                .Map(dest => dest.IsConnected, src => src.WorkerStatus.IsConnected)
                .Map(dest => dest.Scheduled, src => src.WorkerStatus.IsScheduled)
                .Map(dest => dest.Authorized, src => src.WorkerStatus.IsConnected ? src.ConnectorStatus.IsAuthorized : (bool?)null)
                .Map(dest => dest.Running, src => src.WorkerStatus.IsConnected ? src.ConnectorStatus.IsRunning : (bool?)null)
                .Map(dest => dest.CurrentTask, src => (src.WorkerStatus.IsConnected && src.ConnectorStatus.IsRunning) ? src.ConnectorStatus.CurrentTask : null);
    }
}