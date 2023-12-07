using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncServiceFactory
    {
        Task<ISyncService> CreateAsync(Guid networkId, CancellationToken stoppingToken = default);
    }

    internal class SyncServiceFactory : ISyncServiceFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly INetworkPerspectiveCore _networkPerspectiveCoreFacade;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IInteractionsFilterFactory _interactionFilterFactory;
        private readonly IClock _clock;
        private readonly IDataSourceFactory _dataSourceFactory;

        public SyncServiceFactory(ILoggerFactory loggerFactory,
                                  INetworkPerspectiveCore networkPerspectiveCoreFacade,
                                  ISyncHistoryService syncHistoryService,
                                  IInteractionsFilterFactory interactionFilterFactory,
                                  IClock clock,
                                  IDataSourceFactory dataSourceFactory)
        {
            _loggerFactory = loggerFactory;
            _networkPerspectiveCoreFacade = networkPerspectiveCoreFacade;
            _syncHistoryService = syncHistoryService;
            _interactionFilterFactory = interactionFilterFactory;
            _clock = clock;
            _dataSourceFactory = dataSourceFactory;
        }

        public async Task<ISyncService> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var logger = _loggerFactory.CreateLogger<SyncService>();
            var dataSource = await _dataSourceFactory.CreateAsync(networkId, stoppingToken);

            return new SyncService(logger,
                                   dataSource,
                                   _syncHistoryService,
                                   _networkPerspectiveCoreFacade,
                                   _interactionFilterFactory,
                                   _clock);
        }
    }
}