using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IAuthTester
{
    Task<AuthStatus> GetStatusAsync(CancellationToken stoppingToken = default);
}