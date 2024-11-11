using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.Orchestrator.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Persistence.Repositories;

internal class ConnectorPropertyRepository(DbSet<ConnectorPropertyEntity> dbSet) : IConnectorPropertyRepository
{
    public async Task SetAsync(Guid connectorId, IDictionary<string, string> properties, CancellationToken stoppingToken = default)
    {
        try
        {
            await dbSet
                .Where(x => x.ConnectorId == connectorId)
                .ExecuteDeleteAsync(stoppingToken);

            foreach (var property in properties)
            {
                var entity = new ConnectorPropertyEntity
                { 
                    ConnectorId = connectorId,
                    Key = property.Key,
                    Value = property.Value
                };
                await dbSet.AddAsync(entity, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            throw new DbException(ex);
        }
    }
}
