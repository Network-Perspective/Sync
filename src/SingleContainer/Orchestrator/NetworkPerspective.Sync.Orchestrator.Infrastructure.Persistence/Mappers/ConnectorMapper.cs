using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

internal static class ConnectorMapper<TProperties> where TProperties : ConnectorProperties, new()
{
    public static Connector<TProperties> EntityToDomainModel(ConnectorEntity entity)
    {
        var properties = entity.Properties.Select(x => new KeyValuePair<string, string>(x.Key, x.Value));
        var dataSourceProperties = ConnectorProperties.Create<TProperties>(properties);

        var worker = WorkerMapper.EntityToDomainModel(entity.Worker);

        return Connector<TProperties>.Create(entity.Id, worker, entity.NetworkId, dataSourceProperties, entity.CreatedAt);
    }

    public static ConnectorEntity DomainModelToEntity(Connector<TProperties> connector)
    {
        var connectorEntity = new ConnectorEntity
        {
            Id = connector.Id,
            WorkerId = connector.Worker.Id,
            NetworkId = connector.NetworkId,
            CreatedAt = connector.CreatedAt,
        };

        var properties = connector.Properties
            .GetAll()
            .Select(x => new ConnectorPropertyEntity
            {
                Key = x.Key,
                Value = x.Value,
                Connector = connectorEntity,
            })
            .ToList();

        connectorEntity.Properties = properties;

        return connectorEntity;
    }
}