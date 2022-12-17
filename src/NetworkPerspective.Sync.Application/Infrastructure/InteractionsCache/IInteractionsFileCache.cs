using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Interactions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache
{
    public class InteractionsFileCache : IInteractionsCache
    {
        private readonly string _basePath;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<InteractionsFileCache> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public InteractionsFileCache(string basePath, IDataProtector dataProtector, ILogger<InteractionsFileCache> logger)
        {
            _basePath = basePath;
            _dataProtector = dataProtector;
            _logger = logger;
            TryClear();

            Directory.CreateDirectory(_basePath);
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            TryClear();
        }

        public async Task PushInteractionsAsync(IEnumerable<Interaction> interactions, CancellationToken stoppingToken = default)
        {
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                var interactionsGroups = interactions.GroupBy(x => x.Timestamp.Date);

                foreach (var group in interactionsGroups)
                {
                    var filePath = Path.Combine(_basePath, GetInteractionsFileName(group.Key));

                    var result = new HashSet<Interaction>(group, new InteractionEqualityComparer());

                    if (File.Exists(filePath))
                    {
                        var currentEncryptedBytes = await File.ReadAllBytesAsync(filePath, stoppingToken);
                        var currentBytes = _dataProtector.Unprotect(currentEncryptedBytes);
                        var currentContent = Encoding.Unicode.GetString(currentBytes);
                        var currentInteractions = JsonConvert.DeserializeObject<IEnumerable<Interaction>>(currentContent);
                        result.UnionWith(currentInteractions);
                    }

                    var content = JsonConvert.SerializeObject(result);
                    var encryptedContent = _dataProtector.Protect(Encoding.Unicode.GetBytes(content));
                    await File.WriteAllBytesAsync(filePath, encryptedContent, stoppingToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ISet<Interaction>> PullInteractionsAsync(DateTime day, CancellationToken stoppingToken = default)
        {
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                var filePath = Path.Combine(_basePath, GetInteractionsFileName(day));


                if (!File.Exists(filePath))
                    return new HashSet<Interaction>();

                var currentEncryptedBytes = await File.ReadAllBytesAsync(filePath, stoppingToken);
                var currentBytes = _dataProtector.Unprotect(currentEncryptedBytes);
                var content = Encoding.Unicode.GetString(currentBytes);
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

        private void TryClear()
        {
            try
            {
                if (Directory.Exists(_basePath))
                    Directory.Delete(_basePath, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to clear storage '{path}'", _basePath);
            }
        }
    }
}