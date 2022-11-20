using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Interactions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IInteractionsFileStorage : IDisposable
    {
        Task PushInteractionsAsync(ISet<Interaction> interactions, CancellationToken stoppingToken = default);
        Task<ISet<Interaction>> PullInteractionsAsync(DateTime day, CancellationToken stoppingToken = default);
    }

    internal class InteractionsFileStorage : IInteractionsFileStorage
    {
        private readonly string _basePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public InteractionsFileStorage(string basePath)
        {
            _basePath = basePath;
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        public async Task PushInteractionsAsync(ISet<Interaction> interactions, CancellationToken stoppingToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                var interactionsGroups = interactions.GroupBy(x => x.Timestamp.Date);

                foreach (var group in interactionsGroups)
                {
                    var filePath = Path.Combine(_basePath, GetInteractionsFileName(group.Key));
                    var content = JsonConvert.SerializeObject(group, Formatting.Indented);
                    await File.WriteAllTextAsync(filePath, content, stoppingToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ISet<Interaction>> PullInteractionsAsync(DateTime day, CancellationToken stoppingToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                var filePath = Path.Combine(_basePath, GetInteractionsFileName(day));
                var content = await File.ReadAllTextAsync(filePath, stoppingToken);
                var interactions = JsonConvert.DeserializeObject<IEnumerable<Interaction>>(content);
                File.Delete(filePath);
                return new HashSet<Interaction>(interactions, Interaction.EqualityComparer);

            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static string GetInteractionsFileName(DateTime timestamp)
        {
            const string format = "yyyy-MM-dd";

            return timestamp.Date.ToString(format);
        }
    }
}
