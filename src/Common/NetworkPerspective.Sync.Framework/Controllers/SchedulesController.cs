using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Dtos;

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
        /// <response code="200">Synchronization scheduled</response>
        /// <response code="401">Missing or invalid authorization token</response>
        /// <response code="404">Network doesn't exist</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// <response code="200">Synchronization scheduled</response>
        /// <response code="401">Missing or invalid authorization token</response>
        /// <response code="404">Network doesn't exist</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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