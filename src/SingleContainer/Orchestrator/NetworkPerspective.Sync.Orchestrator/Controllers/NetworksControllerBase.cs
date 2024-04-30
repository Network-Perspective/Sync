//using System;
//using System.Threading;
//using System.Threading.Tasks;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//using NetworkPerspective.Sync.Application.Domain.Networks;
//using NetworkPerspective.Sync.Application.Extensions;
//using NetworkPerspective.Sync.Application.Services;
//using NetworkPerspective.Sync.Orchestrator.Extensions;

//namespace NetworkPerspective.Sync.Framework.Controllers
//{
//    [Route("networks")]
//    [Authorize]
//    public abstract class NetworksControllerBase : ControllerBase
//    {
//        private readonly INetworkService _networkService;
//        private readonly ITokenService _tokenService;
//        private readonly ISyncScheduler _syncScheduler;
//        private readonly IStatusLoggerFactory _statusLoggerFactory;
//        private readonly INetworkIdProvider _networkIdProvider;

//        public NetworksControllerBase(INetworkService networkService, ITokenService tokenService, ISyncScheduler syncScheduler, IStatusLoggerFactory statusLoggerFactory, INetworkIdProvider networkIdProvider)
//        {
//            _networkService = networkService;
//            _tokenService = tokenService;
//            _syncScheduler = syncScheduler;
//            _statusLoggerFactory = statusLoggerFactory;
//            _networkIdProvider = networkIdProvider;
//        }

//        protected async Task<Guid> InitializeAsync<TProperties>(TProperties properties, CancellationToken stoppingToken = default) where TProperties : NetworkProperties, new()
//        {
//            await _networkService.AddOrReplace(_networkIdProvider.Get(), properties, stoppingToken);
//            await _tokenService.AddOrReplace(Request.GetServiceAccessToken(), _networkIdProvider.Get(), stoppingToken);
//            await _syncScheduler.AddOrReplaceAsync(_networkIdProvider.Get(), stoppingToken);

//            await _statusLoggerFactory
//                .CreateForNetwork(_networkIdProvider.Get())
//                .LogInfoAsync("Network added", stoppingToken);

//            return _networkIdProvider.Get();
//        }

//        /// <summary>
//        /// Removes network and all it's related data - synchronization history, scheduled jobs, Network Perspective Token, Data source keys
//        /// </summary>
//        /// <param name="stoppingToken">Stopping token</param>
//        /// <response code="200">Network removed</response>
//        /// <response code="401">Missing or invalid authorization token</response>
//        /// <response code="500">Internal server error</response>
//        [HttpDelete]
//        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> RemoveAsync(CancellationToken stoppingToken = default)
//        {
//            await _networkService.EnsureRemovedAsync(_networkIdProvider.Get(), stoppingToken);
//            await _tokenService.EnsureRemovedAsync(_networkIdProvider.Get(), stoppingToken);
//            await _syncScheduler.EnsureRemovedAsync(_networkIdProvider.Get(), stoppingToken);

//            return Ok($"Removed network '{_networkIdProvider.Get()}'");
//        }
//    }
//}