using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services
{
    public class ParallelTask
    {
        private readonly int _tasksCount;
        private readonly IEnumerable<string> _userIds;
        private readonly Func<double, Task> _updateStatus;
        private readonly Func<string, Task> _singleTask;
        private readonly CancellationToken _stoppingToken;
        private int _taskProcessed = 0;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ParallelOptions _parallelOptions;

        private ParallelTask(IEnumerable<string> userIds, Func<double, Task> updateStatus, Func<string, Task> singleTask, CancellationToken stoppingToken = default)
        {
            _tasksCount = userIds.Count();
            _userIds = userIds;
            _updateStatus = updateStatus;
            _singleTask = singleTask;
            _stoppingToken = stoppingToken;
            _parallelOptions = new ParallelOptions { CancellationToken = stoppingToken };
        }

        public async static Task RunAsync(IEnumerable<string> userIds, Func<double, Task> updateStatus, Func<string, Task> singleTask, CancellationToken stoppingToken = default)
        {
            var fetcher = new ParallelTask(userIds, updateStatus, singleTask, stoppingToken);
            await fetcher.RunAsync();
        }

        public async Task RunAsync()
        {
            await _updateStatus(0.0);

            await Parallel.ForEachAsync(_userIds, _parallelOptions, async (user, stoppingToken) =>
            {
                await _singleTask(user);

                await _semaphore.WaitAsync(_stoppingToken);
                try
                {
                    _taskProcessed++;
                    var completionRate = 100.0 * _taskProcessed / _tasksCount;
                    await _updateStatus(completionRate);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }
    }
}