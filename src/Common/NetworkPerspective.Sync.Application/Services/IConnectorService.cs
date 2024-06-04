using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IConnectorService
    {
        Task AddOrReplace<TProperties>(Guid id, TProperties properties, CancellationToken stoppingToken = default) where TProperties : ConnectorProperties, new();
        Task<Connector<TProperties>> GetAsync<TProperties>(Guid id, CancellationToken stoppingtoken = default) where TProperties : ConnectorProperties, new();
        Task EnsureRemovedAsync(Guid id, CancellationToken stoppingtoken = default);
        Task ValidateExists(Guid id, CancellationToken stoppingtoken = default);
    }

    internal class ConnectorService : IConnectorService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger<ConnectorService> _logger;

        public ConnectorService(IUnitOfWorkFactory unitOfWorkFactory, ILogger<ConnectorService> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        public async Task AddOrReplace<TProperties>(Guid id, TProperties properties, CancellationToken stoppingToken = default) where TProperties : ConnectorProperties, new()
        {
            var network = Connector<TProperties>.Create(id, properties, DateTime.UtcNow);

            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetConnectorRepository<TProperties>();

            if (await networkRepository.FindAsync(id, stoppingToken) != null)
            {
                _logger.LogDebug("Connector '{connectorId}' already exists. Removing the old network...", id);
                await networkRepository.RemoveAsync(id, stoppingToken);
            }

            _logger.LogDebug("Adding connector '{connectorId}'...", id);
            await networkRepository.AddAsync(network, stoppingToken);
            await unitOfWork.CommitAsync(stoppingToken);

            _logger.LogDebug("Added connector '{connectorId}'...", id);
        }

        public async Task<Connector<TProperties>> GetAsync<TProperties>(Guid id, CancellationToken stoppingtoken = default) where TProperties : ConnectorProperties, new()
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetConnectorRepository<TProperties>();
            var network = await networkRepository.FindAsync(id, stoppingtoken);

            if (network != null)
                return network;
            else
                throw new ConnectorNotFoundException(id);
        }

        public async Task EnsureRemovedAsync(Guid id, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Removing connector '{connectorId}'...", id);
            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetConnectorRepository<ConnectorProperties>();
            var network = await networkRepository.FindAsync(id, stoppingToken);

            if (network != null)
            {
                await networkRepository.RemoveAsync(id, stoppingToken);
                await unitOfWork.CommitAsync(stoppingToken);
            }

            _logger.LogDebug("Connector '{connectorId}' removed", id);
        }

        public async Task ValidateExists(Guid id, CancellationToken stoppingtoken = default)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var networkRepository = unitOfWork.GetConnectorRepository<ConnectorProperties>();
            var network = await networkRepository.FindAsync(id, stoppingtoken);

            if (network == null)
                throw new ConnectorNotFoundException(id);
        }
    }
}