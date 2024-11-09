using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal class AuthTester(IConnectorInfoProvider connectorInfoProvider, GraphServiceClient graphClient, ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        var connectorInfo = connectorInfoProvider.Get();
        try
        {
            var me = await graphClient.Users.GetAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorInfo.Id);
            return false;
        }
    }
}