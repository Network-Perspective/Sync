using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;
using NetworkPerspective.Sync.Orchestrator.OAuth.Slack;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[Route(AuthPath)]
public class SlackAuthController(IConnectorsService connectorsService, ISlackAuthService authService) : ControllerBase
{
    private const string CallbackPath = "callback";
    private const string AuthPath = "api/connectors/slack-auth";

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

        var useAdminPrivileges = UseAdminPrivileges(connector.Properties);
        var callbackUri = callbackUrl == null ? CreateCallbackUri() : new Uri(callbackUrl);
        var authProcess = new SlackAuthProcess(connectorId, connector.Worker.Name, callbackUri, useAdminPrivileges);

        var result = await authService.StartAuthProcessAsync(authProcess, stoppingToken);

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
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleCallback(string code, string state, CancellationToken stoppingToken = default)
    {
        await authService.HandleAuthorizationCodeCallbackAsync(code, state, stoppingToken);

        return Ok("Auth completed!");
    }

    private static bool UseAdminPrivileges(IDictionary<string, string> properties)
    {
        const string key = "UsesAdminPrivileges";

        if (!properties.ContainsKey(key))
            return false;

        return properties[key] == "true" ? true : false;
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