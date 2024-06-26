﻿using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Slack;
using NetworkPerspective.Sync.Slack.Dtos;

namespace NetworkPerspective.Sync.Slack.Controllers
{
    public class NetworksController : NetworksControllerBase
    {
        public NetworksController(INetworkService networkService, ITokenService tokenService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLoggerFactory, INetworkIdProvider networkIdProvider)
            : base(networkService, tokenService, syncScheduler, statusLoggerFactory, networkIdProvider)
        { }

        /// <summary>
        /// Initializes network
        /// </summary>
        /// <param name="config">Network configuration</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <response code="200">Network added</response>
        /// <response code="401">Missing or invalid authorization token</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAsync([FromBody] NetworkConfigDto config, CancellationToken stoppingToken = default)
        {
            var properties = new SlackNetworkProperties(config.AutoJoinChannels, config.UsesAdminPrivileges, config.SyncChannelsNames, config.ExternalKeyVaultUri);

            var networkId = await InitializeAsync(properties, stoppingToken);

            return Ok($"Added network '{networkId}'");
        }
    }
}