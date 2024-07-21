using System;

using Mapster;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Mappers;

public class SyncRequestConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<SyncRequestDto, SyncRequest>()
            .Ignore(dest => dest.ConnectorId);
    }
}