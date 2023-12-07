using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Dtos;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("schedules")]
    public class SchedulesController : ApiControllerBase
    {
        private readonly INetworkService _networkService;
        private readonly ISyncScheduler _scheduler;
        private readonly ISyncHistoryService _syncHistoryService;
        private readonly IStatusLoggerFactory _statusLoggerFactory;

        public SchedulesController(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, ISyncScheduler scheduler, ISyncHistoryService syncHistoryService, IStatusLoggerFactory statusLoggerFactory) : base(networkPerspectiveCore)
        {
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
            var tokenValidationResponse = await ValidateTokenAsync(stoppingToken);
            await _networkService.ValidateExists(tokenValidationResponse.NetworkId, stoppingToken);

            if (request.OverrideSyncPeriodStart is not null)
                await _syncHistoryService.OverrideSyncStartAsync(tokenValidationResponse.NetworkId, request.OverrideSyncPeriodStart.Value.ToUniversalTime(), stoppingToken);

            await _scheduler.ScheduleAsync(tokenValidationResponse.NetworkId, stoppingToken);
            await _scheduler.TriggerNowAsync(tokenValidationResponse.NetworkId, stoppingToken);

            await _statusLoggerFactory
                .CreateForNetwork(tokenValidationResponse.NetworkId)
                .LogInfoAsync("Schedule started", stoppingToken);

            return Ok($"Scheduled sync {tokenValidationResponse.NetworkId}");
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
            var tokenValidationResponse = await ValidateTokenAsync(stoppingToken);
            await _networkService.ValidateExists(tokenValidationResponse.NetworkId, stoppingToken);

            await _scheduler.UnscheduleAsync(tokenValidationResponse.NetworkId, stoppingToken);
            await _scheduler.InterruptNowAsync(tokenValidationResponse.NetworkId, stoppingToken);

            await _statusLoggerFactory
                .CreateForNetwork(tokenValidationResponse.NetworkId)
                .LogInfoAsync("Schedule stopped", stoppingToken);

            return Ok($"Unscheduled sync {tokenValidationResponse.NetworkId}");
        }
    }
}