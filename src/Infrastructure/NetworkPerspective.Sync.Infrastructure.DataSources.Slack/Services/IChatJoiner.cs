using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services
{
    internal interface IChatJoiner
    {
        Task JoinAsync(CancellationToken stoppingToken = default);
    }
}