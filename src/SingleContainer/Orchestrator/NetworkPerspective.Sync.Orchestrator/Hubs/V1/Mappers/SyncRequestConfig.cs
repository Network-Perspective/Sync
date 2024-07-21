using System;

using Mapster;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

public class SyncRequestConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<SyncRequest, SyncRequestDto>()
                .Ignore(x => x.CorrelationId)
                .AfterMapping(x => x.CorrelationId = Guid.NewGuid());
    }
}