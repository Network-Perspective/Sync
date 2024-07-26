using Mapster;

namespace NetworkPerspective.Sync.Orchestrator.Hubs.V1.Mappers
{
    public class HubV1MapsterConfig
    {
        public static void RegisterMappings(TypeAdapterConfig config)
        {
            new StartSyncRequestConfig().Register(config);
        }
    }
}