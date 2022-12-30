using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Domain
{
    public class Batcher<T>
    {
        private List<T> _objects = new List<T>();
        private readonly int _batchSize;
        private BatchReadyCallback<T> _callback = _ => Task.CompletedTask;

        public Batcher(int batchSize)
        {
            if (batchSize < 1)
                throw new ArgumentException($"{nameof(batchSize)} needs to be positive number");

            _batchSize = batchSize;
        }

        public void OnBatchReady(BatchReadyCallback<T> callback)
            => _callback = callback;

        public async Task AddRangeAsync(IEnumerable<T> objects, CancellationToken stoppingToken = default)
        {
            _objects.AddRange(objects);

            while (_objects.Count >= _batchSize)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                await _callback(new BatchReadyEventArgs<T>(_objects.Take(_batchSize)));
                _objects = _objects.Skip(_batchSize).ToList();
            }
        }

        public async Task FlushAsync()
        {
            if (_objects.Any())
            {
                await _callback(new BatchReadyEventArgs<T>(_objects));
                _objects = Enumerable.Empty<T>().ToList();
            }
        }
    }
}