using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IStatusLogger
{
    public Task AddLogAsync(Guid connectorId, string message, StatusLogLevel level, CancellationToken stoppingToken = default);
}

internal class StatusLogger : IStatusLogger
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<StatusLogger> _logger;


    public StatusLogger(IUnitOfWork unitOfWork, IClock clock, ILogger<StatusLogger> logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task AddLogAsync(Guid connectorId, string message, StatusLogLevel level, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Adding new {type} to connector '{connectorId}': '{log}'", typeof(StatusLog), connectorId, message);
            var log = StatusLog.Create(connectorId, message, level, _clock.UtcNow());

            await _unitOfWork
                .GetStatusLogRepository()
                .AddAsync(log, stoppingToken);

            await _unitOfWork.CommitAsync(stoppingToken);

            _logger.LogDebug("Added new {type} to persistence for connector '{connectorId}'", typeof(StatusLog), connectorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to add {type} to persistence for connector '{connectorId}'", typeof(StatusLog), connectorId);
        }
    }
}