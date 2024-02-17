using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IAuthTester
    {
        Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default);
    }

    public class DummyAuthTester : IAuthTester
    {
        public Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
        {
            return Task.FromResult(true);
        }
    }
}