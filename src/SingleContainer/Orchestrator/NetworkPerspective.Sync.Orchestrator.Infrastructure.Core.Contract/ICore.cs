using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;

public interface ICore
{
    Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default);
}