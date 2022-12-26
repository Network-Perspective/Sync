using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Application.Services
{
    public class ParallelFetcherWithStorage
    {
        private readonly int _tasksCount;
        private readonly IInteractionsStorage _interactionStorage;
        private readonly IEnumerable<string> _userIds;
        private readonly Func<double, Task> _updateStatus;
        private readonly Func<string, Task<ISet<Interaction>>> _singleTask;
        private readonly CancellationToken _stoppingToken;
        private int _taskProcessed = 0;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ParallelOptions _parallelOptions;

        private ParallelFetcherWithStorage(IInteractionsStorage interactionStorage, IEnumerable<string> userIds, Func<double, Task> updateStatus, Func<string, Task<ISet<Interaction>>> singleTask, CancellationToken stoppingToken = default)
        {
            _tasksCount = userIds.Count();
            _interactionStorage = interactionStorage;
            _userIds = userIds;
            _updateStatus = updateStatus;
            _singleTask = singleTask;
            _stoppingToken = stoppingToken;
            _parallelOptions = new ParallelOptions { CancellationToken = stoppingToken };
        }

        public async static Task FetchAsync(IInteractionsStorage interactionsStorage, IEnumerable<string> userIds, Func<double, Task> updateStatus, Func<string, Task<ISet<Interaction>>> singleTask, CancellationToken stoppingToken = default)
        {
            var fetcher = new ParallelFetcherWithStorage(interactionsStorage, userIds, updateStatus, singleTask, stoppingToken);
            await fetcher.FetchAsync();
        }

        public async Task FetchAsync()
        {
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            await _updateStatus(0.0);

            await Parallel.ForEachAsync(_userIds, _parallelOptions, async (user, stoppingToken) =>
            {
                var interactions = await _singleTask(user);
                await UpdateStatus();
                await _interactionStorage.PushInteractionsAsync(interactions, stoppingToken);
            });
        }

        private async Task UpdateStatus()
        {
            try
            {
                await _semaphore.WaitAsync(_stoppingToken);
                _taskProcessed++;
                var completionRate = 100.0 * _taskProcessed / _tasksCount;
                await _updateStatus(completionRate);
            }
            finally
            {
                _semaphore.Release();

            }
        }
    }
}