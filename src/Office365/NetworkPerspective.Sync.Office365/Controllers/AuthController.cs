using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Office365.Controllers
{
    [Route(AuthPath)]
    public class AuthController : ControllerBase
    {
        private const string CallbackPath = "callback";
        private const string AuthPath = "auth";

        private readonly IMicrosoftAuthService _authService;
        private readonly IConnectorService _connectorService;
        private readonly IConnectorInfoProvider _connectorInfoProvider;

        public AuthController(IMicrosoftAuthService authService, IConnectorService connectorService, IConnectorInfoProvider connectorInfoProvider)
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
        public async Task<IActionResult> AuthorizeAsync(string callbackUrl = null, CancellationToken stoppingToken = default)
        {
            var connectorInfo = _connectorInfoProvider.Get();
            await _connectorService.ValidateExists(connectorInfo.Id, stoppingToken);

            var callbackUri = callbackUrl == null ? CreateCallbackUri() : new Uri(callbackUrl);
            var authProcess = new AuthProcess(connectorInfo.Id, callbackUri);

            var result = await _authService.StartAuthProcessAsync(authProcess, stoppingToken);

            return Ok(result.MicrosoftAuthUri);
        }

        /// <summary>
        /// OAuth callback
        /// </summary>
        /// <param name="tenant">Tenant id</param>
        /// <param name="state">Anti-forgery unique value</param>
        /// <param name="error">Error</param>
        /// <param name="error_description">Error description</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <response code="200">OAuth process completed</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">State does not match any initialized OAuth process</response>
        /// <response code="500">Internal server error</response>        
        [HttpGet(CallbackPath)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleCallback(Guid tenant, string state, string error, string error_description, CancellationToken stoppingToken = default)
        {
            if (error is not null || error_description is not null)
                throw new OAuthException(error, error_description);

            await _authService.HandleCallbackAsync(tenant, state, stoppingToken);

            return Ok("Admin consent completed!");
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