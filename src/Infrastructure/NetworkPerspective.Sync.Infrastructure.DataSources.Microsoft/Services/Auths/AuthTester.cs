using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;

internal class AuthTester(IConnectorContextAccessor connectorInfoProvider, IMicrosoftClientFactory clientFactory, ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        var connectorInfo = connectorInfoProvider.Context;
        try
        {
            var client = await clientFactory.GetMicrosoftClientAsync(stoppingToken);
            var me = await client.Users.GetAsync(cancellationToken: stoppingToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorInfo.ConnectorId);
            return false;
        }
    }
}