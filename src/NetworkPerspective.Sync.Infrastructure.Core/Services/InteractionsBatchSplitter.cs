using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Infrastructure.Core.Services
{
    public interface IInteractionsBatchSplitter
    {
        int BatchSize { get; set; }
        long? BufferSize { get; set; }

        event InteractionsBatchSplitter.BatchIsReadyAsyncEventHandler BatchIsReadyAsync;
        Task FlushAsync();
        Task PushInteractionAsync(HashedInteraction interaction);
    }

    /// <summary>
    /// Splits interactions into batches of a specified size (approx)
    /// Ensures that the same events are in the same batch (as it is a requirement down the stream) i.e.
    /// all interactions with the same <EventId, When> should fall in one batch
    /// </summary>
    public class InteractionsBatchSplitter : IInteractionsBatchSplitter
    {
        public delegate Task BatchIsReadyAsyncEventHandler(object sender, BatchIsReadyEventArgs e);

        public event BatchIsReadyAsyncEventHandler BatchIsReadyAsync;
        
        public int BatchSize { get; set; } = 10000;
        public long? BufferSize { get; set; } = 100000;

        private readonly PriorityQueue<HashedInteraction, Tuple<DateTime, string>> _queue;
        private int _batchNo = 0;

        public InteractionsBatchSplitter()
        {
             _queue = new PriorityQueue<HashedInteraction, Tuple<DateTime, string>>();
        }
        
        public async Task PushInteractionAsync(HashedInteraction interaction)
        {
            if (interaction.When == null) return;

            _queue.Enqueue(interaction, GetKey(interaction));

            if (BufferSize != null && (_queue.Count >= BufferSize))
            {
                await EmitNewBatchAsync();
            }
        }

        private async Task EmitNewBatchAsync()
        {
            var batch = new List<HashedInteraction>(BatchSize);            
            HashedInteraction nextItem = null, prevItem = null;
            int size = 0;

            while (true)
            {
                if (_queue.Count == 0) break;

                prevItem = nextItem;                
                nextItem = _queue.Peek();                                

                if (size >= BatchSize && !string.Equals(prevItem.EventId, nextItem.EventId, StringComparison.InvariantCultureIgnoreCase)) break;
                                
                batch.Add(_queue.Dequeue());
                size++;
            }

            if (batch.Any())
            {
                await BatchIsReadyAsync?.Invoke(this, new BatchIsReadyEventArgs(_batchNo, batch));
                _batchNo++;
            }
        }

        private readonly long _ticksIn10Minutes = TimeSpan.FromMinutes(10).Ticks;
            
        private Tuple<DateTime, string> GetKey(HashedInteraction interaction)
        {
            if (interaction == null) return null;

            var dt = interaction.When.Value.UtcDateTime;
            var roundDate = new DateTime((dt.Ticks + _ticksIn10Minutes - 1) / _ticksIn10Minutes * _ticksIn10Minutes, DateTimeKind.Utc);

            return new Tuple<DateTime, string>(roundDate, interaction.EventId);
        }

        public async Task FlushAsync()
        {            
            while (_queue.Count > 0)
            {
                await EmitNewBatchAsync();
            }
        }
    }

    public class BatchIsReadyEventArgs
    {
        public BatchIsReadyEventArgs(int batchNo, ICollection<HashedInteraction> interactions)
        {
            BatchNo = batchNo;
            Interactions = interactions;
        }
        
        public int BatchNo { get; }
        public ICollection<HashedInteraction> Interactions { get; }            
    }
}
