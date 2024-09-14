using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Sync
{
    public class ParallelSyncTask<T>
    {
        public delegate Task<SingleTaskResult> SingleSyncTask(T userId);

        private readonly int _tasksCount;
        private int _tasksProcessedCount = 0;

        private readonly List<Exception> _exceptions = new List<Exception>();
        private long _totalInteractionsCount = 0;
        private readonly IEnumerable<T> _ids;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Func<double, Task> _updateStatus;
        private readonly SingleSyncTask _singleTask;

        private readonly ParallelOptions _parallelOptions;
        private readonly CancellationToken _stoppingToken;

        private ParallelSyncTask(IEnumerable<T> ids, Func<double, Task> updateStatus, SingleSyncTask singleTask, CancellationToken stoppingToken = default)
        {
            _tasksCount = ids.Count();
            _ids = ids;
            _updateStatus = updateStatus;
            _singleTask = singleTask;
            _stoppingToken = stoppingToken;
            _parallelOptions = new ParallelOptions { CancellationToken = stoppingToken };
        }

        public static async Task<SyncResult> RunAsync(IEnumerable<T> ids, Func<double, Task> updateStatus, SingleSyncTask singleTask, CancellationToken stoppingToken = default)
        {
            var fetcher = new ParallelSyncTask<T>(ids, updateStatus, singleTask, stoppingToken);
            return await fetcher.RunAsync();
        }

        public async Task<SyncResult> RunAsync()
        {
            await _updateStatus(0.0);

            await Parallel.ForEachAsync(_ids, _parallelOptions, async (id, stoppingToken) =>
            {
                await RunSingleTask(id);
                await TryReportProgress();
            });

            return new SyncResult(_tasksCount, _totalInteractionsCount, _exceptions);
        }

        private async Task RunSingleTask(T id)
        {
            try
            {
                var result = await _singleTask(id);
                await TryAddInteractionsCount(result.InteractionsCount);

            }
            catch (Exception ex)
            {
                await TryRecordException(ex);
            }
        }

        private async Task TryAddInteractionsCount(int interactionsCount)
        {
            await _semaphore.WaitAsync(_stoppingToken);

            try
            {
                _totalInteractionsCount += interactionsCount;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task TryRecordException(Exception ex)
        {
            await _semaphore.WaitAsync(_stoppingToken);

            try
            {
                _exceptions.Add(ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task TryReportProgress()
        {
            await _semaphore.WaitAsync(_stoppingToken);

            try
            {
                _tasksProcessedCount++;
                var completionRate = 100.0 * _tasksProcessedCount / _tasksCount;
                await _updateStatus(completionRate);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}