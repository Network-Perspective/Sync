using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Slack;
using NetworkPerspective.Sync.Slack.Dtos;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Slack.Controllers
{
    public class NetworksController : NetworksControllerBase
    {
        public NetworksController(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, ITokenService tokenService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLoggerFactory, INetworkIdInitializer networkIdInitializer)
            : base(networkPerspectiveCore, networkService, tokenService, syncScheduler, statusLoggerFactory, networkIdInitializer)
        { }

        /// <summary>
        /// Initializes network
        /// </summary>
        /// <param name="config">Network configuration</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Synchronization scheduled", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> AddAsync([FromBody] NetworkConfigDto config, CancellationToken stoppingToken = default)
        {
            var properties = new SlackNetworkProperties(config.AutoJoinChannels, config.UsesAdminPrivileges, config.SyncChannelsNames, config.ExternalKeyVaultUri);

            var networkId = await InitializeAsync(properties, stoppingToken);

            return Ok($"Added network '{networkId}'");
        }
    }
}