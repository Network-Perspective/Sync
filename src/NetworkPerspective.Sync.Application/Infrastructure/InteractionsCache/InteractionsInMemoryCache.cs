using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache
{
    internal class InteractionsInMemoryCache : IInteractionsCache
    {
        private readonly ILogger<InteractionsInMemoryCache> _logger;

        public InteractionsInMemoryCache(ILogger<InteractionsInMemoryCache> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<ISet<Interaction>> PullInteractionsAsync(DateTime day, CancellationToken stoppingToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PushInteractionsAsync(IEnumerable<Interaction> interactions, CancellationToken stoppingToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
