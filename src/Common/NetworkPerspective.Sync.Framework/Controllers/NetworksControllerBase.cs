using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Extensions;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("networks")]
    [Authorize]
    public abstract class NetworksControllerBase : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly ITokenService _tokenService;
        private readonly ISyncScheduler _syncScheduler;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly IConnectorInfoProvider _connectorInfoProvider;

        public NetworksControllerBase(IConnectorService connectorService, ITokenService tokenService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLoggerFactory, IConnectorInfoProvider connectorInfoProvider)
        {
            _connectorService = connectorService;
            _tokenService = tokenService;
            _syncScheduler = syncScheduler;
            _statusLoggerFactory = statusLoggerFactory;
            _connectorInfoProvider = connectorInfoProvider;
        }

        protected async Task<Guid> InitializeAsync<TProperties>(TProperties properties, CancellationToken stoppingToken = default) where TProperties : ConnectorProperties, new()
        {
            var connectorInfo = _connectorInfoProvider.Get();

            await _connectorService.AddOrReplace(connectorInfo.Id, properties, stoppingToken);
            await _tokenService.AddOrReplace(Request.GetServiceAccessToken(), connectorInfo.Id, stoppingToken);
            await _syncScheduler.AddOrReplaceAsync(connectorInfo, stoppingToken);

            await _statusLoggerFactory
                .CreateForConnector(connectorInfo.Id)
                .LogInfoAsync("Connector added", stoppingToken);

            return connectorInfo.Id;
        }

        /// <summary>
        /// Removes network and all it's related data - synchronization history, scheduled jobs, Network Perspective Token, Data source keys
        /// </summary>
        /// <param name="stoppingToken">Stopping token</param>
        /// <response code="200">Network removed</response>
        /// <response code="401">Missing or invalid authorization token</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveAsync(CancellationToken stoppingToken = default)
        {
            var connectorInfo = _connectorInfoProvider.Get();

            await _connectorService.EnsureRemovedAsync(connectorInfo.Id, stoppingToken);
            await _tokenService.EnsureRemovedAsync(connectorInfo.Id, stoppingToken);
            await _syncScheduler.EnsureRemovedAsync(connectorInfo, stoppingToken);

            return Ok($"Removed connector '{connectorInfo.Id}'");
        }
    }
}