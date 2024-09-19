using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

internal class AuthTester : IAuthTester
{
    public Task<bool> IsAuthorizedAsync(IDictionary<string, string> connectorProperties, CancellationToken stoppingToken = default)
        => Task.FromResult(true); // TODO implementation
}