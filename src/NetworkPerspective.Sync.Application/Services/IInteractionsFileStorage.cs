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
    public interface IInteractionsStorage : IDisposable
    {
        Task PushInteractionsAsync(ISet<Interaction> interactions, CancellationToken stoppingToken = default);
        Task<ISet<Interaction>> PullInteractionsAsync(DateTime day, CancellationToken stoppingToken = default);
    }

    public class InteractionsFileStorage : IInteractionsStorage
    {
        private readonly string _basePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public InteractionsFileStorage(string basePath)
        {
            _basePath = basePath;

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
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

                    var result = new HashSet<Interaction>(group, new InteractionEqualityComparer());

                    if (File.Exists(filePath))
                    {
                        var currentContent = await File.ReadAllTextAsync(filePath, stoppingToken);
                        var currentInteractions = JsonConvert.DeserializeObject<IEnumerable<Interaction>>(currentContent);
                        result.UnionWith(currentInteractions);
                    }

                    var content = JsonConvert.SerializeObject(result);
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


                if (!File.Exists(filePath))
                    return new HashSet<Interaction>();

                var content = await File.ReadAllTextAsync(filePath, stoppingToken);
                var interactions = JsonConvert.DeserializeObject<IEnumerable<Interaction>>(content);
                File.Delete(filePath);
                return new HashSet<Interaction>(interactions, new InteractionEqualityComparer());

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