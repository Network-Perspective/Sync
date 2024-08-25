using Mapster;

using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Mappers;

public class StatusConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Status, StatusDto>()
                .Map(dest => dest.IsConnected, src => src.WorkerStatus.IsConnected)
                .Map(dest => dest.Scheduled, src => src.WorkerStatus.IsScheduled)
                .Map(dest => dest.Authorized, src => src.ConnectorStatus.IsAuthorized)
                .Map(dest => dest.Running, src => src.ConnectorStatus.IsRunning)
                .Map(dest => dest.CurrentTask, src => src.ConnectorStatus.CurrentTask);
    }
}