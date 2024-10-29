using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services;

internal class AuthTester(IJiraAuthorizedFacade jiraFacade, ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            var resources = await jiraFacade.GetAccessibleResourcesAsync(stoppingToken);

            if (resources.Count == 0)
            {
                logger.LogWarning("Token is valid but there are no accessible resources");
                return false;
            }

            var currentUser = await jiraFacade.GetCurrentUserAsync(resources.First().Id, stoppingToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Not authorized");
            return false;
        }
    }
}