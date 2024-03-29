using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.SingleContainer.Host.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages;

namespace NetworkPerspective.Sync.SingleContainer.Host.Controllers;

public class NetworksController : Controller
{
    private readonly IRemoteConnectorClient _remoteConnector;

    public NetworksController(IRemoteConnectorClient remoteConnector)
    {
        _remoteConnector = remoteConnector;
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAsync()
    {
        await _remoteConnector.InvokeConnectorAsync("Client-123", new AddNetwork(Guid.NewGuid()));
        return Ok($"Added network");
    }
}