using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;

namespace NetworkPerspective.Sync.SingleContainer.Host.Controllers;

public class NetworksController(IRemoteConnectorClient remoteConnector) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAsync()
    {
        await remoteConnector.InvokeConnectorAsync("Client-123", new AddNetwork(Guid.NewGuid()));
        return Ok($"Added network");
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckAsync()
    {
        var result = await remoteConnector
            .CallConnectorAsync<IsAuthenticatedResult>("Client-123",
                new IsAuthenticated("are we? " + DateTime.Now.ToUniversalTime()));
        return Ok(result.Message);
    }
}