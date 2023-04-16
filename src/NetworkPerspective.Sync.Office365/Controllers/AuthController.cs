using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Controllers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Office365.Controllers
{
    [Route(AuthPath)]
    public class AuthController : ApiControllerBase
    {
        private const string CallbackPath = "callback";
        private const string AuthPath = "auth";

        private readonly IMicrosoftAuthService _authService;
        private readonly INetworkService _networkService;

        public AuthController(IMicrosoftAuthService authService, INetworkPerspectiveCore networkPerspectiveCore, INetworkService networkService) : base(networkPerspectiveCore)
        {
            _authService = authService;
            _networkService = networkService;
        }

        /// <summary>
        /// Initialize OAuth process
        /// </summary>
        /// <param name="callbackUrl">Code redirection url, default the request url. Use it in case application is behind reverse proxy</param>
        /// <param name="stoppingToken">Stopping token</param>
        /// <returns>Result</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Initialized OAuth process", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Missing or invalid authorization token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Network doesn't exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> AuthorizeAsync(string callbackUrl = null, CancellationToken stoppingToken = default)
        {
            var tokenValidationResponse = await ValidateTokenAsync(stoppingToken);
            await _networkService.ValidateExists(tokenValidationResponse.NetworkId, stoppingToken);

            var callbackUri = callbackUrl == null ? CreateCallbackUri() : new Uri(callbackUrl);
            var authProcess = new AuthProcess(tokenValidationResponse.NetworkId, callbackUri);

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
        /// <returns>Result</returns>
        [HttpGet(CallbackPath)]
        [SwaggerResponse(StatusCodes.Status200OK, "OAuth process completed", typeof(string))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "State does not match any initialized OAuth process")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> HandleCallback(Guid tenant, string state, string error, string error_description, CancellationToken stoppingToken = default)
        {
            if (error is not null || error_description is not null)
                throw new OAuthException(error, error_description);

            await _authService.HandleAuthorizationCodeCallbackAsync(tenant, state, stoppingToken);

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