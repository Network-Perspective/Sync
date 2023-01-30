using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Slack;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Slack.Controllers
{
    public class NetworksController : NetworksControllerBase
    {
        public NetworksController(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, ITokenService tokenService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLoggerFactory)
            : base(networkPerspectiveCore, networkService, tokenService, syncScheduler, statusLoggerFactory)
        { }

        /// <summary>
        /// Initializes network
        /// </summary>
        /// <param name="autoJoinChannels">Enable/disable automatic channel join</param>
        /// <param name="syncChannelsNames">Enable/disable channels names synchronization</param>
        /// <param name="externalKeyVaultUri">External Key Vault Uri (optional)</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Synchronization scheduled", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Add(bool autoJoinChannels = true, bool syncChannelsNames = false, Uri externalKeyVaultUri = null, CancellationToken stoppingToken = default)
        {
            var properties = new SlackNetworkProperties(autoJoinChannels, syncChannelsNames, externalKeyVaultUri);

            var networkId = await InitializeAsync(properties, stoppingToken);

            return Ok($"Added network '{networkId}'");
        }
    }
}