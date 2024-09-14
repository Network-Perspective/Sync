using Mapster;

using NetworkPerspective.Sync.Orchestrator.Controllers.Mappers;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class ControllersMapsterConfig
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        new ConnectorConfig().Register(config);
        new StatusConfig().Register(config);
    }
}