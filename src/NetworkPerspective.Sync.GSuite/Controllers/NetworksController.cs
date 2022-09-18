using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Google;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.GSuite.Controllers
{
    public class NetworksController : NetworksControllerBase
    {
        public NetworksController(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, ITokenService authService, ISyncScheduler syncScheduler, IStatusLogger statusLogger) : base(networkPerspectiveCore, networkService, authService, syncScheduler, statusLogger)
        { }

        /// <summary>
        /// Initializes network
        /// </summary>
        /// <param name="adminEmail">GSuite admin email address</param>
        /// <param name="externalKeyVaultUri">External Key Vault Uri (optional)</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Network added", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Add(string adminEmail, Uri externalKeyVaultUri = null, CancellationToken stoppingToken = default)
        {
            var properties = new GoogleNetworkProperties(adminEmail, externalKeyVaultUri);

            var networkId = await InitializeAsync(properties, stoppingToken);

            return Ok($"Added network '{networkId}'");
        }
    }
}