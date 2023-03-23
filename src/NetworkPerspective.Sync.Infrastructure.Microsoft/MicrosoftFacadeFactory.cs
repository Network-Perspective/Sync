using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal class MicrosoftFacadeFactory : IDataSourceFactory
    {
        private readonly IMicrosoftClientFactory _microsoftClientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public MicrosoftFacadeFactory(IMicrosoftClientFactory microsoftClientFactory, ILoggerFactory loggerFactory)
        {
            _microsoftClientFactory = microsoftClientFactory;
            _loggerFactory = loggerFactory;
        }

        public async Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var microsoftClient = await _microsoftClientFactory.GetMicrosoftClientAsync(networkId, stoppingToken);

            var usersClient = new UsersClient(microsoftClient, _loggerFactory.CreateLogger<UsersClient>());
            return new MicrosoftFacade(usersClient, _loggerFactory.CreateLogger<MicrosoftFacade>());
        }
    }
}