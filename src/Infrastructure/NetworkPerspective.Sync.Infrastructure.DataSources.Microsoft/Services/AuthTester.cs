using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal class AuthTester(ILogger<AuthTester> logger) : IAuthTester
{
    public Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        logger.LogWarning($"{nameof(AuthTester)} for Microsoft has no implementation yet. Returning true, but it's not a real check.");
        return Task.FromResult(true);
    }
}