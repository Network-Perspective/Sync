using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IOAuthService
{
    Task<InitializeOAuthResult> InitializeOAuthAsync(OAuthContext context, CancellationToken stoppingToken = default);
    Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
}
