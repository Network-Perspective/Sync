using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack;
using NetworkPerspective.Sync.Infrastructure.Slack.Models;
using NetworkPerspective.Sync.Infrastructure.Slack.Services;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Slack.Controllers
{
    [Route(AuthPath)]
    public class AuthController : ControllerBase
    {
        private const string CallbackPath = "callback";
        private const string AuthPath = "auth";

        private readonly ISlackAuthService _authService;
        private readonly INetworkService _networkService;
        private readonly INetworkIdProvider _networkIdProvider;

        public AuthController(ISlackAuthService authService, INetworkService networkService, INetworkIdProvider networkIdProvider)
        {
            _authService = authService;
            _networkService = networkService;
            _networkIdProvider = networkIdProvider;
        }

        /// <summary>
        /// Initialize OAuth process
        /// </summary>
        /// <param name="callbackUrl">Code redirection url, default the request url. Use it in case application is behind reverse proxy</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, "Initialized OAuth process", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> SignIn(string callbackUrl = null, CancellationToken stoppingToken = default)
        {
            await _networkService.ValidateExists(_networkIdProvider.Get(), stoppingToken);

            var network = await _networkService.GetAsync<SlackNetworkProperties>(_networkIdProvider.Get(), stoppingToken);
            var callbackUri = callbackUrl == null ? CreateCallbackUri() : new Uri(callbackUrl);
            var authProcess = new AuthProcess(_networkIdProvider.Get(), callbackUri, network.Properties.UsesAdminPrivileges);

            var result = await _authService.StartAuthProcessAsync(authProcess, stoppingToken);

            return Ok(result.SlackAuthUri);
        }

        /// <summary>
        /// OAuth callback
        /// </summary>
        /// <param name="code">Authorization code</param>
        /// <param name="state">Anti-forgery unique value</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpGet(CallbackPath)]
        [SwaggerResponse(StatusCodes.Status200OK, "OAuth process completed", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "State does not match any initialized OAuth process")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> HandleCallback(string code, string state, CancellationToken stoppingToken = default)
        {
            await _authService.HandleAuthorizationCodeCallbackAsync(code, state, stoppingToken);

            return Ok("Auth completed!");
        }

        private Uri CreateCallbackUri()
        {
            var callbackUrlBuilder = new UriBuilder();
            callbackUrlBuilder.Scheme = "https";
            callbackUrlBuilder.Host = HttpContext.Request.Host.Host;
            if (HttpContext.Request.Host.Port.HasValue)
                callbackUrlBuilder.Port = HttpContext.Request.Host.Port.Value;
            callbackUrlBuilder.Path = string.Join('/', AuthPath, CallbackPath);
            return callbackUrlBuilder.Uri;
        }
    }
}