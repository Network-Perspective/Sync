using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IAuthTester
{
    Task<bool> IsAuthorizedAsync(IDictionary<string, string> networkProperties, CancellationToken stoppingToken = default);
}