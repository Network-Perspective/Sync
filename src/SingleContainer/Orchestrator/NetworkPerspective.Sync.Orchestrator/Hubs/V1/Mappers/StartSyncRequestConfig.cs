﻿using Mapster;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers;

public class StartSyncRequestConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<SyncContext, SyncRequest>()
                .Map(dest => dest.Start, src => src.TimeRange.Start)
                .Map(dest => dest.End, src => src.TimeRange.End)
                .Map(dest => dest.AccessToken, src => src.AccessToken.ToSystemString())
                .Map(dest => dest.Connector.Id, src => src.ConnectorId)
                .Map(dest => dest.Connector.Type, src => src.ConnectorType)
                .Map(dest => dest.Connector.Properties, src => src.NetworkProperties);
    }
}