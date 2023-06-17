using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal class AuthTester : IAuthTester
    {
        public Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
            => Task.FromResult(true);
    }
}
