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
        private int batchCount = 0;
        private BatchReadyCallback<T> _callback = _ => Task.CompletedTask;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

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
            await _semaphoreSlim.WaitAsync(stoppingToken);
            try
            {
                _objects.AddRange(objects);

                while (_objects.Count >= _batchSize)
                {
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    batchCount++;

                    await _callback(new BatchReadyEventArgs<T>(_objects.Take(_batchSize), batchCount));
                    _objects = _objects.Skip(_batchSize).ToList();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task FlushAsync()
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_objects.Any())
                {
                    batchCount++;

                    await _callback(new BatchReadyEventArgs<T>(_objects, batchCount));
                    _objects = Enumerable.Empty<T>().ToList();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}