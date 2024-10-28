using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route(AuthPath)]
public class JiraAuthController(IConnectorsService connectorsService, IWorkerRouter workerRouter, IMemoryCache cache) : ControllerBase
{
    private const string CallbackPath = "callback";
    private const string AuthPath = "api/connectors/jira-auth";

    /// <summary>
    /// Initialize OAuth process
    /// </summary>
    /// <param name="connectorId">Id of Connector to authorize</param>
    /// <param name="callbackUrl">Code redirection url, default the request url. Use it in case application is behind reverse proxy</param>
    /// <param name="stoppingToken">Stopping token</param>
    /// <response code="200">Initialized OAuth process</response>
    /// <response code="401">Missing or invalid authorization token</response>
    /// <response code="404">Network doesn't exist</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiKeyAuthOptions.DefaultScheme)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Authorize([FromQuery] Guid connectorId, string callbackUrl = null, CancellationToken stoppingToken = default)
    {
        var connector = await connectorsService.GetAsync(connectorId, stoppingToken);

        var callbackUri = callbackUrl is null
            ? CreateCallbackUri()
            : new Uri(callbackUrl);

        var result = await workerRouter.InitializeOAuthAsync(connector.Worker.Name, connectorId, connector.Type, callbackUri.ToString(), connector.Properties);
        cache.Set(result.State, connector.Worker.Name, result.StateExpirationTimestamp);

        return Ok(result.AuthUri);
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleCallback(string code, string state, CancellationToken stoppingToken = default)
    {
        if (!cache.TryGetValue(state, out string workerName))
            throw new OAuthException("State does not match initialized value");

        await workerRouter.HandleOAuthCallbackAsync(workerName, code, state);

        return Ok("Auth completed!");
    }

    private Uri CreateCallbackUri()
    {
        var callbackUrlBuilder = new UriBuilder
        {
            Scheme = "https",
            Host = HttpContext.Request.Host.Host
        };

        if (HttpContext.Request.Host.Port.HasValue)
            callbackUrlBuilder.Port = HttpContext.Request.Host.Port.Value;

        callbackUrlBuilder.Path = string.Join('/', AuthPath, CallbackPath);
        return callbackUrlBuilder.Uri;
    }
}