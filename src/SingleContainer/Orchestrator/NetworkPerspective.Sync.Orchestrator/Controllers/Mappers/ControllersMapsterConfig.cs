using Mapster;

using NetworkPerspective.Sync.Orchestrator.Controllers.Mappers;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class ControllersMapsterConfig
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.RequireDestinationMemberSource = true;

        new ConnectorConfig().Register(config);
        new SyncRequestConfig().Register(config);
    }
}