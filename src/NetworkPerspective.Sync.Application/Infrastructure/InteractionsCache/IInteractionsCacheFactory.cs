using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache
{
    public interface IInteractionsCacheFactory
    {
        public Task<IInteractionsCache> CreateAsync(Guid networkId, CancellationToken stoppingToken = default);
    }

    internal class InteractionsCacheFactory : IInteractionsCacheFactory
    {
        private readonly INetworkService _networkService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILoggerFactory _loggerFactory;

        public InteractionsCacheFactory(INetworkService networkService, IDataProtectionProvider dataProtectionProvider, ILoggerFactory loggerFactory)
        {
            _networkService = networkService;
            _dataProtectionProvider = dataProtectionProvider;
            _loggerFactory = loggerFactory;
        }

        public async Task<IInteractionsCache> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var network = await _networkService.GetAsync<NetworkProperties>(networkId, stoppingToken);

            if(network.Properties.UseDurableInteractionsCache)
            {
                var path = Path.Combine("tmp", networkId.ToString());
                var dataProtector = _dataProtectionProvider.CreateProtector(networkId.ToString());
                var logger = _loggerFactory.CreateLogger<InteractionsFileCache>();
                return new InteractionsFileCache(path, dataProtector, logger);
            }
            else
            {
                var logger = _loggerFactory.CreateLogger<InteractionsInMemoryCache>();
                return new InteractionsInMemoryCache(logger);
            }
        }
    }
}
