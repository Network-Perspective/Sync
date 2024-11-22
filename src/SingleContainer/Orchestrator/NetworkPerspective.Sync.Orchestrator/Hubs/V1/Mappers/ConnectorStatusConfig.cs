using Mapster;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

public class ConnectorStatusConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<ConnectorStatusDto, ConnectorStatus>()
                .ConstructUsing(x => CreateDomainStatus(x));
    }

    private static ConnectorStatus CreateDomainStatus(ConnectorStatusDto dto)
    {
        if (!dto.IsRunning)
            return ConnectorStatus.Idle(dto.IsAuthorized);

        var currentTask = ConnectorTaskStatus.Create(dto.CurrentTaskCaption, dto.CurrentTaskDescription, dto.CurrentTaskCompletionRate);
        return ConnectorStatus.Running(dto.IsAuthorized, currentTask);
    }
}