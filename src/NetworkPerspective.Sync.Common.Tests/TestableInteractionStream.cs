using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Common.Tests
{
    public class TestableInteractionStream : IInteractionsStream
    {
        public IList<IList<Interaction>> SentBatches { get; } = new List<IList<Interaction>>();

        public IList<Interaction> SentInteractions
            => SentBatches.SelectMany(x => x).ToList();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        public async Task SendAsync(IEnumerable<Interaction> interactions)
        {
            await _semaphore.WaitAsync();

            try
            {
                SentBatches.Add(interactions.ToList());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Reset()
            => SentBatches.Clear();
    }
}