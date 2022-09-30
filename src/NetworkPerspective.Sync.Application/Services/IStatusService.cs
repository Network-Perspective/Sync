using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IStatusService
    {
        public Task<Status> GetStatusAsync(Guid networkId, CancellationToken stoppingToken = default);
    }

    internal class StatusService : IStatusService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ITokenService _tokenService;
        private readonly IDataSourceFactory _dataSourceFactory;
        private readonly ISyncScheduler _scheduler;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<StatusService> _logger;

        public StatusService(IUnitOfWorkFactory unitOfWorkFactory, ITokenService tokenService, IDataSourceFactory dataSourceFactory, ISyncScheduler scheduler, ITasksStatusesCache tasksStatusesCache, ILogger<StatusService> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _tokenService = tokenService;
            _dataSourceFactory = dataSourceFactory;
            _scheduler = scheduler;
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<Status> GetStatusAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Checking status of network '{networkId}'", networkId);

            using var unitOfWork = _unitOfWorkFactory
                .Create();

            var logs = await unitOfWork
                .GetStatusLogRepository()
                .GetListAsync(networkId, stoppingToken);

            var isAuthorizedToCoreApp = await _tokenService
                .HasValidAsync(networkId, stoppingToken);

            _logger.LogDebug("Network '{networkId}' authorization status to Core app is '{status}'", networkId, isAuthorizedToCoreApp);

            var dataSource = await _dataSourceFactory.CreateAsync(networkId, stoppingToken);
            var isAuthorizedToDataSource = await dataSource.IsAuthorized(networkId, stoppingToken);
            var isRunning = await _scheduler.IsRunningAsync(networkId, stoppingToken);

            _logger.LogDebug("Network '{networkId}' authorization status to data source is '{status}'", networkId, isAuthorizedToDataSource);

            return new Status
            {
                Authorized = isAuthorizedToCoreApp && isAuthorizedToDataSource,
                Scheduled = await _scheduler.IsScheduledAsync(networkId, stoppingToken),
                Running = isRunning,
                CurrentTask = isRunning ? await _tasksStatusesCache.GetStatusAsync(networkId, stoppingToken) : null,
                Logs = logs
            };
        }
    }
}