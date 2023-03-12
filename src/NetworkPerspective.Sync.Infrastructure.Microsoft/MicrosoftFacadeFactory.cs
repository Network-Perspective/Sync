using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal class MicrosoftFacadeFactory : IDataSourceFactory
    {
        public Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(new MicrosoftFacade() as IDataSource);
        }
    }
}