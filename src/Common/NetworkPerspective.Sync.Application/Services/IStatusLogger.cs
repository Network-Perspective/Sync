using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IStatusLogger
    {
        public Task AddLogAsync(string message, StatusLogLevel level, CancellationToken stoppingToken = default);
    }

    internal class StatusLogger : IStatusLogger
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IClock _clock;
        private readonly ILogger<StatusLogger> _logger;

        private readonly Guid _connectorId;

        public StatusLogger(Guid connectorId, IUnitOfWorkFactory unitOfWorkFactory, IClock clock, ILogger<StatusLogger> logger)
        {
            _connectorId = connectorId;
            _unitOfWorkFactory = unitOfWorkFactory;
            _clock = clock;
            _logger = logger;
        }

        public async Task AddLogAsync(string message, StatusLogLevel level, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Adding new {type} to connector '{connectorId}': '{log}'", typeof(StatusLog), _connectorId, message);
                var log = StatusLog.Create(_connectorId, message, level, _clock.UtcNow());

                using var unitOfWork = _unitOfWorkFactory
                    .Create();

                await unitOfWork
                    .GetStatusLogRepository()
                    .AddAsync(log, stoppingToken);

                await unitOfWork.CommitAsync(stoppingToken);

                _logger.LogDebug("Added new {type} to persistence for connector '{connectorId}'", typeof(StatusLog), _connectorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to add {type} to persistence for connector '{connectorId}'", typeof(StatusLog), _connectorId);
            }
        }
    }
}