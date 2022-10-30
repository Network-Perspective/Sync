using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Core.Services
{
    public interface IInteractionsBatchSplitter
    {
        void OnBatchIsReady(InteractionsBatchSplitter.BatchIsReadyCallback callback);
        Task FlushAsync();
        Task PushInteractionAsync(HashedInteraction interaction);
    }

    public class InteractionsBatchSplitterConfig
    {
        public int BatchSize { get; set; } = 10000;
        public long BufferSize { get; set; } = 100000;
    }

    /// <summary>
    /// Splits interactions into batches of a specified size (approx)
    /// Ensures that the same events are in the same batch (as it is a requirement down the stream) i.e.
    /// all interactions with the same <EventId, When> should fall in one batch
    /// </summary>
    public class InteractionsBatchSplitter : IInteractionsBatchSplitter
    {
        public delegate Task BatchIsReadyCallback(BatchIsReadyArgs args);

        private BatchIsReadyCallback _batchIsReadyCallback = _ => Task.CompletedTask;

        private readonly PriorityQueue<HashedInteraction, Tuple<DateTime, string>> _queue;
        private readonly InteractionsBatchSplitterConfig _config;
        private int _batchNo = 0;

        public InteractionsBatchSplitter(IOptions<InteractionsBatchSplitterConfig> config)
        {
            _queue = new PriorityQueue<HashedInteraction, Tuple<DateTime, string>>();
            _config = config.Value;
        }

        public async Task PushInteractionAsync(HashedInteraction interaction)
        {
            if (interaction.When == null) return;

            _queue.Enqueue(interaction, GetKey(interaction));

            if (_config.BufferSize != null && (_queue.Count >= _config.BufferSize))
            {
                await HandleNewBatchAsync();
            }
        }

        private async Task HandleNewBatchAsync()
        {
            var batch = new List<HashedInteraction>(_config.BatchSize);
            HashedInteraction nextItem = null, prevItem = null;
            int size = 0;

            while (true)
            {
                if (_queue.Count == 0) break;

                prevItem = nextItem;
                nextItem = _queue.Peek();

                if (size >= _config.BatchSize && !string.Equals(prevItem.EventId, nextItem.EventId, StringComparison.InvariantCultureIgnoreCase)) break;

                batch.Add(_queue.Dequeue());
                size++;
            }

            if (batch.Any())
            {
                await _batchIsReadyCallback?.Invoke(new BatchIsReadyArgs(_batchNo, batch));
                _batchNo++;
            }
        }

        private Tuple<DateTime, string> GetKey(HashedInteraction interaction)
        {
            if (interaction == null) return null;

            var roundDate = interaction.When.Value.UtcDateTime.Bucket(TimeSpan.FromMinutes(10));

            return new Tuple<DateTime, string>(roundDate, interaction.EventId);
        }

        public async Task FlushAsync()
        {
            while (_queue.Count > 0)
            {
                await HandleNewBatchAsync();
            }
        }

        public void OnBatchIsReady(BatchIsReadyCallback callback)
            => _batchIsReadyCallback = callback;
    }

    public class BatchIsReadyArgs
    {
        public BatchIsReadyArgs(int batchNo, ICollection<HashedInteraction> interactions)
        {
            BatchNo = batchNo;
            Interactions = interactions;
        }

        public int BatchNo { get; }
        public ICollection<HashedInteraction> Interactions { get; }
    }
}