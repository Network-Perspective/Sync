using System.Linq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

internal static class ConnectorMapper
{
    public static Connector EntityToDomainModel(ConnectorEntity entity)
    {
        var properties = entity.Properties.ToDictionary(x => x.Key, y => y.Value);
        var worker = WorkerMapper.EntityToDomainModel(entity.Worker);
        return new Connector(entity.Id, entity.Type, properties, worker, entity.NetworkId, entity.CreatedAt);
    }

    public static ConnectorEntity DomainModelToEntity(Connector connector)
    {
        var connectorEntity = new ConnectorEntity
        {
            Id = connector.Id,
            Type = connector.Type,
            WorkerId = connector.Worker.Id,
            NetworkId = connector.NetworkId,
            CreatedAt = connector.CreatedAt,
            Properties = connector.Properties
                .Select(x => new ConnectorPropertyEntity
                {
                    Key = x.Key,
                    Value = x.Value,
                    ConnectorId = connector.Id,
                })
                .ToList()
        };

        return connectorEntity;
    }
}