using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

internal class AuthTester(ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            // TODO: Implement authorization logic
            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Not authorized");
            return false;
        }
    }
}