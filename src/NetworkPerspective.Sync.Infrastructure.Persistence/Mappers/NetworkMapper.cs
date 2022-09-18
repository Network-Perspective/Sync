using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Mappers
{
    internal static class NetworkMapper<TProperties> where TProperties : NetworkProperties, new()
    {
        public static Network<TProperties> EntityToDomainModel(NetworkEntity entity)
        {
            var properties = entity.Properties.Select(x => new KeyValuePair<string, string>(x.Key, x.Value));
            var networkProperties = NetworkProperties.Create<TProperties>(properties);

            return Network<TProperties>.Create(entity.Id, networkProperties, entity.CreatedAt);
        }

        public static NetworkEntity DomainModelToEntity(Network<TProperties> network)
        {
            var networkEntity = new NetworkEntity
            {
                Id = network.NetworkId,
                CreatedAt = network.CreatedAt,
            };

            var properties = network.Properties
                .GetAll()
                .Select(x => new NetworkPropertyEntity
                {
                    Key = x.Key,
                    Value = x.Value,
                    Network = networkEntity,
                })
                .ToList();

            networkEntity.Properties = properties;

            return networkEntity;
        }
    }
}