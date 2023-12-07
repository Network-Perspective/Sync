using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface INetworkService
    {
        Task AddOrReplace<TProperties>(Guid networkId, TProperties properties, CancellationToken stoppingToken = default) where TProperties : NetworkProperties, new();
        Task<Network<TProperties>> GetAsync<TProperties>(Guid networkId, CancellationToken stoppingtoken = default) where TProperties : NetworkProperties, new();
        Task EnsureRemovedAsync(Guid networkId, CancellationToken stoppingtoken = default);
        Task ValidateExists(Guid networkId, CancellationToken stoppingtoken = default);
    }

    internal class NetworkService : INetworkService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger<NetworkService> _logger;

        public NetworkService(IUnitOfWorkFactory unitOfWorkFactory, ILogger<NetworkService> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        public async Task AddOrReplace<TProperties>(Guid networkId, TProperties properties, CancellationToken stoppingToken = default) where TProperties : NetworkProperties, new()
        {
            var network = Network<TProperties>.Create(networkId, properties, DateTime.UtcNow);

            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetNetworkRepository<TProperties>();

            if (await networkRepository.FindAsync(networkId, stoppingToken) != null)
            {
                _logger.LogDebug("Network '{networkId}' already exists. Removing the old network...", networkId);
                await networkRepository.RemoveAsync(networkId, stoppingToken);
            }

            _logger.LogDebug("Adding network '{networkId}'...", networkId);
            await networkRepository.AddAsync(network, stoppingToken);
            await unitOfWork.CommitAsync(stoppingToken);

            _logger.LogDebug("Added network '{networkId}'...", networkId);
        }

        public async Task<Network<TProperties>> GetAsync<TProperties>(Guid networkId, CancellationToken stoppingtoken = default) where TProperties : NetworkProperties, new()
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetNetworkRepository<TProperties>();
            var network = await networkRepository.FindAsync(networkId, stoppingtoken);

            if (network != null)
                return network;
            else
                throw new NetworkNotFoundException(networkId);
        }

        public async Task EnsureRemovedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Removing network '{networkId}'...", networkId);
            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetNetworkRepository<NetworkProperties>();
            var network = await networkRepository.FindAsync(networkId, stoppingToken);

            if (network != null)
            {
                await networkRepository.RemoveAsync(networkId, stoppingToken);
                await unitOfWork.CommitAsync(stoppingToken);
            }

            _logger.LogDebug("Network '{networkId}' removed", networkId);
        }

        public async Task ValidateExists(Guid networkId, CancellationToken stoppingtoken = default)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetNetworkRepository<NetworkProperties>();
            var network = await networkRepository.FindAsync(networkId, stoppingtoken);

            if (network == null)
                throw new NetworkNotFoundException(networkId);
        }
    }
}