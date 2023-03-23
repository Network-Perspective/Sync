using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Microsoft;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Office365.Controllers
{
    public class NetworksController : NetworksControllerBase
    {
        public NetworksController(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, ITokenService authService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLogger) : base(networkPerspectiveCore, networkService, authService, syncScheduler, statusLogger)
        { }

        /// <summary>
        /// Initializes network
        /// </summary>
        /// <param name="externalKeyVaultUri">External Key Vault Uri (optional)</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Network added", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Add(Uri externalKeyVaultUri = null, CancellationToken stoppingToken = default)
        {
            var properties = new MicrosoftNetworkProperties(externalKeyVaultUri);

            var networkId = await InitializeAsync(properties, stoppingToken);

            return Ok($"Added network '{networkId}'");
        }
    }
}