using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Infrastructure.DataSources
{
    public interface IDataSourceFactory
    {
        Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default);
    }
}