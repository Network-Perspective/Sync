using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories
{
    public interface INetworkRepository<TProperties> where TProperties : NetworkProperties, new()
    {
        Task AddAsync(Network<TProperties> network, CancellationToken stoppingToken = default);
        Task RemoveAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task<Network<TProperties>> FindAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task<IEnumerable<Network<TProperties>>> GetAllAsync(CancellationToken stoppingToken = default);
    }
}