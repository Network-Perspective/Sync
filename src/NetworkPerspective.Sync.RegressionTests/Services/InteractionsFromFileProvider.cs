using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Core;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.RegressionTests.Services
{
    internal class InteractionsFromFileProvider : IInteractionsProvider
    {
        private readonly string _dirPath;

        public InteractionsFromFileProvider(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                throw new DirectoryNotFoundException($"Directory {dirPath} not found");

            _dirPath = dirPath;
        }

        public async Task<IList<HashedInteraction>> GetInteractionsAsync(CancellationToken stoppingToken = default)
        {
            var filesNames = Directory.GetFiles(_dirPath, "*.json", SearchOption.TopDirectoryOnly);

            if (!filesNames.Any())
                throw new Exception($"Directory {_dirPath} contains no json files");

            var result = new List<HashedInteraction>();

            foreach (var fileName in filesNames)
                result.AddRange(await GetInteractionsFromFile(fileName, stoppingToken));

            return result;
        }

        private async Task<IList<HashedInteraction>> GetInteractionsFromFile(string fileName, CancellationToken stoppingToken)
        {
            var rawContent = await File.ReadAllTextAsync(fileName, stoppingToken);

            return JsonConvert.DeserializeObject<IList<HashedInteraction>>(rawContent);
        }
    }
}