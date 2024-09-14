using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;

namespace NetworkPerspective.Sync.RegressionTests.Services
{
    internal interface IInteractionsProvider
    {
        Task<IList<HashedInteraction>> GetInteractionsAsync(CancellationToken stoppingToken);
    }
}