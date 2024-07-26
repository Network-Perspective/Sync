using Mapster;

namespace NetworkPerspective.Sync.Orchestrator.Mappers;

public static class ControllersMapsterConfig
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        new ConnectorConfig().Register(config);
    }
}