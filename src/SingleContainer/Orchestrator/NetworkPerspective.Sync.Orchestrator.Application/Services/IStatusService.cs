using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IStatusService
{
    public Task<Status> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default);
}

internal class StatusService : IStatusService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<StatusService> _logger;

    public StatusService(IUnitOfWork unitOfWork, ITokenService tokenService, ILogger<StatusService> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Status> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Checking status of connector '{connectorId}'", connectorId);

        var logs = await _unitOfWork
            .GetStatusLogRepository()
            .GetListAsync(connectorId, stoppingToken);

        return new Status
        {
            Authorized = true,
            Scheduled = true,
            Running = false,
            CurrentTask = SingleTaskStatus.Empty,
            Logs = logs
        };
    }
}