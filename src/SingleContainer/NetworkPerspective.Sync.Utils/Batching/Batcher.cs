using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Utils.Batching;

public class Batcher<T>
{
    private List<T> _objects = new();
    private readonly int _batchSize;
    private int _batchCount = 0;
    private BatchReadyCallback<T> _callback = _ => Task.CompletedTask;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

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

                _batchCount++;

                await _callback(new BatchReadyEventArgs<T>(_objects.Take(_batchSize), _batchCount));
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
                _batchCount++;

                await _callback(new BatchReadyEventArgs<T>(_objects, _batchCount));
                _objects = Enumerable.Empty<T>().ToList();
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}