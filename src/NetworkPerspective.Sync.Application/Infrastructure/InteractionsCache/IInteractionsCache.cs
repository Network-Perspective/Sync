using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache
{
    public interface IInteractionsCache : IDisposable
    {
        Task PushInteractionsAsync(IEnumerable<Interaction> interactions, CancellationToken stoppingToken = default);
        Task<ISet<Interaction>> PullInteractionsAsync(DateTime day, CancellationToken stoppingToken = default);
    }
}