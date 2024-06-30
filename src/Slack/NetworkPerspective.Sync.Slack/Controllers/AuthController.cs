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

namespace NetworkPerspective.Sync.Slack.Controllers
{
    [Route(AuthPath)]
    public class AuthController : ControllerBase
    {
        private const string CallbackPath = "callback";
        private const string AuthPath = "auth";

        private readonly ISlackAuthService _authService;
        private readonly IConnectorService _connectorService;
        private readonly IConnectorInfoProvider _connectorInfoProvider;

        public AuthController(ISlackAuthService authService, IConnectorService connectorService, IConnectorInfoProvider connectorInfoProvider)
        {
            _authService = authService;
            _connectorService = connectorService;
            _connectorInfoProvider = connectorInfoProvider;
        }

        /// <summary>
        /// Initialize OAuth process
        /// </summary>
        /// <param name="callbackUrl">Code redirection url, default the request url. Use it in case application is behind reverse proxy</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <response code="200">Initialized OAuth process</response>
        /// <response code="401">Missing or invalid authorization token</response>
        /// <response code="404">Network doesn't exist</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignIn(string callbackUrl = null, CancellationToken stoppingToken = default)
        {
            var connectorInfo = _connectorInfoProvider.Get();
            await _connectorService.ValidateExists(connectorInfo.Id, stoppingToken);

            var network = await _connectorService.GetAsync<SlackConnectorProperties>(connectorInfo.Id, stoppingToken);
            var callbackUri = callbackUrl == null ? CreateCallbackUri() : new Uri(callbackUrl);
            var authProcess = new AuthProcess(connectorInfo.Id, callbackUri, network.Properties.UsesAdminPrivileges);

            var result = await _authService.StartAuthProcessAsync(authProcess, stoppingToken);

            return Ok(result.SlackAuthUri);
        }

        /// <summary>
        /// OAuth callback
        /// </summary>
        /// <param name="code">Authorization code</param>
        /// <param name="state">Anti-forgery unique value</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <response code="200">OAuth process completed</response>
        /// <response code="401">State does not match any initialized OAuth process</response>
        /// <response code="500">Internal server error</response>
        [HttpGet(CallbackPath)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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