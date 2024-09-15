using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;

public interface IJiraAuthService
{
    Task<JiraAuthStartProcessResult> StartAuthProcessAsync(JiraAuthProcess authProcess, CancellationToken stoppingToken = default);
    Task HandleCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
}