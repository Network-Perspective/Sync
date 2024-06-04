using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IStatusService
    {
        public Task<Status> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default);
    }

    internal class StatusService : IStatusService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ITokenService _tokenService;
        private readonly ISyncScheduler _scheduler;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IAuthTester _authTester;
        private readonly ILogger<StatusService> _logger;

        public StatusService(IUnitOfWorkFactory unitOfWorkFactory, ITokenService tokenService, ISyncScheduler scheduler, ITasksStatusesCache tasksStatusesCache, IAuthTester authTester, ILogger<StatusService> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _tokenService = tokenService;
            _scheduler = scheduler;
            _tasksStatusesCache = tasksStatusesCache;
            _authTester = authTester;
            _logger = logger;
        }

        public async Task<Status> GetStatusAsync(Guid connectorId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Checking status of connector '{connectorId}'", connectorId);

            using var unitOfWork = _unitOfWorkFactory
                .Create();

            var logs = await unitOfWork
                .GetStatusLogRepository()
                .GetListAsync(connectorId, stoppingToken);

            var isAuthorizedToCoreApp = await _tokenService
                .HasValidAsync(connectorId, stoppingToken);

            _logger.LogDebug("Connector '{connectorId}' authorization status to Core app is '{status}'", connectorId, isAuthorizedToCoreApp);

            var isAuthorizedToDataSource = await _authTester.IsAuthorizedAsync(stoppingToken);
            var isRunning = await _scheduler.IsRunningAsync(connectorId, stoppingToken);

            _logger.LogDebug("Connector '{connectorId}' authorization status to data source is '{status}'", connectorId, isAuthorizedToDataSource);

            return new Status
            {
                Authorized = isAuthorizedToCoreApp && isAuthorizedToDataSource,
                Scheduled = await _scheduler.IsScheduledAsync(connectorId, stoppingToken),
                Running = isRunning,
                CurrentTask = isRunning ? await _tasksStatusesCache.GetStatusAsync(connectorId, stoppingToken) : null,
                Logs = logs
            };
        }
    }
}