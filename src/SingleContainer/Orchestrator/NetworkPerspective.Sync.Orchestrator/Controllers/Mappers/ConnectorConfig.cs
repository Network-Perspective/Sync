using Mapster;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public class ConnectorConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Connector, ConnectorDto>()
                .Map(dest => dest.WorkerId, src => src.Worker.Id);
    }
}