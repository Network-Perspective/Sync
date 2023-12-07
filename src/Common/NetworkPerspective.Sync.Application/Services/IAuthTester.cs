using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IAuthTester
    {
        Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default);
    }
}