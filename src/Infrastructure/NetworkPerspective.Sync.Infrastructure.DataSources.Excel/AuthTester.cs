using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

internal class AuthTester : IAuthTester
{
    public Task<AuthStatus> GetStatusAsync(CancellationToken stoppingToken = default)
    {
        // TODO: Implement authorization logic
        return Task.FromResult(AuthStatus.Empty);
    }
}