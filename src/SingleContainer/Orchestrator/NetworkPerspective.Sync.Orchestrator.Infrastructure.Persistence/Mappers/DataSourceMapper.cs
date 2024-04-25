using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

internal static class DataSourceMapper<TProperties> where TProperties : DataSourceProperties, new()
{
    public static DataSource<TProperties> EntityToDomainModel(DataSourceEntity entity)
    {
        var properties = entity.Properties.Select(x => new KeyValuePair<string, string>(x.Key, x.Value));
        var dataSourceProperties = DataSourceProperties.Create<TProperties>(properties);

        return DataSource<TProperties>.Create(entity.Id, entity.ConnectorId, entity.NetworkId, dataSourceProperties, entity.CreatedAt);
    }

    public static DataSourceEntity DomainModelToEntity(DataSource<TProperties> dataSource)
    {
        var networkEntity = new DataSourceEntity
        {
            Id = dataSource.NetworkId,
            ConnectorId = dataSource.ConnectorId,
            NetworkId = dataSource.NetworkId,
            CreatedAt = dataSource.CreatedAt,
        };

        var properties = dataSource.Properties
            .GetAll()
            .Select(x => new DataSourcePropertyEntity
            {
                Key = x.Key,
                Value = x.Value,
                DataSource = networkEntity,
            })
            .ToList();

        networkEntity.Properties = properties;

        return networkEntity;
    }
}