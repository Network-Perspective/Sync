using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.StatusLogs;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IStatusLogger
    {
        public Task AddLogAsync(StatusLog log, CancellationToken stoppingToken = default);
    }

    internal class StatusLogger : IStatusLogger
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger<StatusLogger> _logger;

        public StatusLogger(IUnitOfWorkFactory unitOfWorkFactory, ILogger<StatusLogger> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        public async Task AddLogAsync(StatusLog log, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Adding new {type} to network '{networkId}': '{log}'", typeof(StatusLog), log.NetworkId, log.Message);

                using var unitOfWork = _unitOfWorkFactory
                    .Create();

                await unitOfWork
                    .GetStatusLogRepository()
                    .AddAsync(log, stoppingToken);

                await unitOfWork.CommitAsync(stoppingToken);

                _logger.LogDebug("Added new {type} to persistence", typeof(StatusLog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to add {type} to persistence", typeof(StatusLog));
            }
        }
    }
}