using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("networks")]
    public abstract class NetworksControllerBase : ApiControllerBase
    {
        private readonly INetworkService _networkService;
        private readonly ITokenService _tokenService;
        private readonly ISyncScheduler _syncScheduler;
        private readonly IStatusLoggerFactory _statusLoggerFactory;

        public NetworksControllerBase(INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService, ITokenService tokenService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLoggerFactory) : base(networkPerspectiveCore)
        {
            _networkService = networkService;
            _tokenService = tokenService;
            _syncScheduler = syncScheduler;
            _statusLoggerFactory = statusLoggerFactory;
        }

        protected async Task<Guid> InitializeAsync<TProperties>(TProperties properties, CancellationToken stoppingToken = default) where TProperties : NetworkProperties, new()
        {
            var tokenValidationResponse = await ValidateTokenAsync(stoppingToken);

            await _networkService.AddOrReplace(tokenValidationResponse.NetworkId, properties, stoppingToken);
            await _tokenService.AddOrReplace(GetAccessToken(), tokenValidationResponse.NetworkId, stoppingToken);
            await _syncScheduler.AddOrReplaceAsync(tokenValidationResponse.NetworkId, stoppingToken);

            await _statusLoggerFactory
                .CreateForNetwork(tokenValidationResponse.NetworkId)
                .LogInfoAsync("Network added", stoppingToken);

            return tokenValidationResponse.NetworkId;
        }

        /// <summary>
        /// Removes network and all it's related data - synchronization history, scheduled jobs, Network Perspective Token, Data source keys
        /// </summary>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpDelete]
        [SwaggerResponse(StatusCodes.Status200OK, "Network removed", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> RemoveAsync(CancellationToken stoppingToken = default)
        {
            var tokenValidationResponse = await ValidateTokenAsync(stoppingToken);

            await _networkService.EnsureRemovedAsync(tokenValidationResponse.NetworkId, stoppingToken);
            await _tokenService.EnsureRemovedAsync(tokenValidationResponse.NetworkId, stoppingToken);
            await _syncScheduler.EnsureRemovedAsync(tokenValidationResponse.NetworkId, stoppingToken);

            return Ok($"Removed network '{tokenValidationResponse.NetworkId}'");
        }
    }
}