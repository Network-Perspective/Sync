using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Domain.Sync
{
    public class ParallelSyncTask
    {
        public delegate Task<SingleTaskResult> SingleSyncTask(string userId);

        private readonly int _usersCount;
        private int _usersProcessedCount = 0;

        private readonly List<Exception> _exceptions = new List<Exception>();
        private long _totalInteractionsCount = 0;
        private readonly IEnumerable<string> _userIds;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Func<double, Task> _updateStatus;
        private readonly SingleSyncTask _singleTask;

        private readonly ParallelOptions _parallelOptions;
        private readonly CancellationToken _stoppingToken;

        private ParallelSyncTask(IEnumerable<string> userIds, Func<double, Task> updateStatus, SingleSyncTask singleTask, CancellationToken stoppingToken = default)
        {
            _usersCount = userIds.Count();
            _userIds = userIds;
            _updateStatus = updateStatus;
            _singleTask = singleTask;
            _stoppingToken = stoppingToken;
            _parallelOptions = new ParallelOptions { CancellationToken = stoppingToken };
        }

        public static async Task<SyncResult> RunAsync(IEnumerable<string> userIds, Func<double, Task> updateStatus, SingleSyncTask singleTask, CancellationToken stoppingToken = default)
        {
            var fetcher = new ParallelSyncTask(userIds, updateStatus, singleTask, stoppingToken);
            return await fetcher.RunAsync();
        }

        public async Task<SyncResult> RunAsync()
        {
            await _updateStatus(0.0);

            await Parallel.ForEachAsync(_userIds, _parallelOptions, async (userId, stoppingToken) =>
            {
                await RunSingleTask(userId);
                await TryReportProgress();
            });

            return new SyncResult(_usersCount, _totalInteractionsCount, _exceptions);
        }

        private async Task RunSingleTask(string userId)
        {
            try
            {
                var result = await _singleTask(userId);
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
                _usersProcessedCount++;
                var completionRate = 100.0 * _usersProcessedCount / _usersCount;
                await _updateStatus(completionRate);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}