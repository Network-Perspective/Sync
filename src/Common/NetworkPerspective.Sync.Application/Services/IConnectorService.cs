using System;
using System.Collections.Generic;
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
        Task<IEnumerable<KeyValuePair<string, string>>> GetProperties(Guid id, CancellationToken stoppingtoken = default);
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
            var connectorRepository = unitOfWork.GetConnectorRepository<TProperties>();

            if (await connectorRepository.FindAsync(id, stoppingToken) != null)
            {
                _logger.LogDebug("Connector '{connectorId}' already exists. Removing the old network...", id);
                await connectorRepository.RemoveAsync(id, stoppingToken);
            }

            _logger.LogDebug("Adding connector '{connectorId}'...", id);
            await connectorRepository.AddAsync(network, stoppingToken);
            await unitOfWork.CommitAsync(stoppingToken);

            _logger.LogDebug("Added connector '{connectorId}'...", id);
        }

        public async Task<Connector<TProperties>> GetAsync<TProperties>(Guid id, CancellationToken stoppingtoken = default) where TProperties : ConnectorProperties, new()
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var connectorRepository = unitOfWork.GetConnectorRepository<TProperties>();
            var connector = await connectorRepository.FindAsync(id, stoppingtoken);

            if (connector != null)
                return connector;
            else
                throw new ConnectorNotFoundException(id);
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetProperties(Guid id, CancellationToken stoppingtoken = default)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var connectorRepository = unitOfWork.GetConnectorRepository<ConnectorProperties>();
            var properties = await connectorRepository.FindPropertiesAsync(id, stoppingtoken);

            if (properties != null)
                return properties;
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