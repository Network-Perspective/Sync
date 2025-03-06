using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services;

internal class AuthTester(IJiraAuthorizedFacade jiraFacade, ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<AuthStatus> GetStatusAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            var resources = await jiraFacade.GetAccessibleResourcesAsync(stoppingToken);

            if (resources.Count == 0)
            {
                logger.LogWarning("Token is valid but there are no accessible resources");
                return AuthStatus.Create(false);
            }

            var currentUser = await jiraFacade.GetCurrentUserAsync(resources.First().Id, stoppingToken);
            return AuthStatus.Create(true);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Not authorized");
            return AuthStatus.Create(false);
        }
    }
}