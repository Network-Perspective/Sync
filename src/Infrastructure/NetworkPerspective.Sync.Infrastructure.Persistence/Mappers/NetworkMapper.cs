using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Mappers
{
    internal static class NetworkMapper<TProperties> where TProperties : ConnectorProperties, new()
    {
        public static Connector<TProperties> EntityToDomainModel(ConnectorEntity entity)
        {
            var properties = entity.Properties.Select(x => new KeyValuePair<string, string>(x.Key, x.Value));
            var networkProperties = ConnectorProperties.Create<TProperties>(properties);

            return Connector<TProperties>.Create(entity.Id, networkProperties, entity.CreatedAt);
        }

        public static ConnectorEntity DomainModelToEntity(Connector<TProperties> network)
        {
            var networkEntity = new ConnectorEntity
            {
                Id = network.Id,
                CreatedAt = network.CreatedAt,
            };

            var properties = network.Properties
                .GetAll()
                .Select(x => new ConnectorPropertyEntity
                {
                    Key = x.Key,
                    Value = x.Value,
                    Connector = networkEntity,
                })
                .ToList();

            networkEntity.Properties = properties;

            return networkEntity;
        }
    }
}