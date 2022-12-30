using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetworkPerspective.Sync.Infrastructure.Core.Services
{
    internal class FileDataWriter
    {
        private readonly string _directory;

        public FileDataWriter(string directory)
        {
            _directory = directory;
        }

        public async Task WriteAsync<T>(T data, string fileName, CancellationToken stoppingToken = default)
        {
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            var payload = JsonConvert.SerializeObject(data, Formatting.Indented, new[] { new StringEnumConverter() });
            await File.WriteAllTextAsync(Path.Combine(_directory, fileName), payload, stoppingToken);
        }
    }
}