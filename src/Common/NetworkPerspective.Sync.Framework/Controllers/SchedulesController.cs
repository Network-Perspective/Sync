using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Dtos;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("schedules")]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly INetworkIdProvider _networkIdProvider;
        private readonly INetworkService _networkService;
        private readonly ISyncScheduler _scheduler;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IStatusLoggerFactory _statusLoggerFactory;

        public SchedulesController(INetworkIdProvider networkIdProvider, INetworkService networkService, ISyncScheduler scheduler, ISyncHistoryService syncHistoryService, IStatusLoggerFactory statusLoggerFactory)
        {
            _networkIdProvider = networkIdProvider;
            _networkService = networkService;
            _scheduler = scheduler;
            _syncHistoryService = syncHistoryService;
            _statusLoggerFactory = statusLoggerFactory;
        }

        /// <summary>
        /// Schedules synchronization job for given network to run at midnight, and also triggers the synchronization to run now
        /// </summary>
        /// <param name="request">Scheduler properties</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Synchronization scheduled", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> StartAsync([FromBody] SchedulerStartDto request, CancellationToken stoppingToken = default)
        {
            await _networkService.ValidateExists(_networkIdProvider.Get(), stoppingToken);

            if (request.OverrideSyncPeriodStart is not null)
                await _syncHistoryService.OverrideSyncStartAsync(_networkIdProvider.Get(), request.OverrideSyncPeriodStart.Value.ToUniversalTime(), stoppingToken);

            await _scheduler.ScheduleAsync(_networkIdProvider.Get(), stoppingToken);
            await _scheduler.TriggerNowAsync(_networkIdProvider.Get(), stoppingToken);

            await _statusLoggerFactory
                .CreateForNetwork(_networkIdProvider.Get())
                .LogInfoAsync("Schedule started", stoppingToken);

            return Ok($"Scheduled sync {_networkIdProvider.Get()}");
        }

        /// <summary>
        /// Unschedules synchronization job for given network
        /// </summary>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpDelete]
        [SwaggerResponse(StatusCodes.Status200OK, "Synchronization unscheduled", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> StopAsync(CancellationToken stoppingToken = default)
        {
            await _networkService.ValidateExists(_networkIdProvider.Get(), stoppingToken);

            await _scheduler.UnscheduleAsync(_networkIdProvider.Get(), stoppingToken);
            await _scheduler.InterruptNowAsync(_networkIdProvider.Get(), stoppingToken);

            await _statusLoggerFactory
                .CreateForNetwork(_networkIdProvider.Get())
                .LogInfoAsync("Schedule stopped", stoppingToken);

            return Ok($"Unscheduled sync {_networkIdProvider.Get()}");
        }
    }
}