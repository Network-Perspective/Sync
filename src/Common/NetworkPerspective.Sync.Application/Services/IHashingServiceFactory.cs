using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IHashingServiceFactory
    {
        Task<IHashingService> CreateAsync(ISecretRepository secretRepository, CancellationToken stoppingToken = default);
    }

    internal sealed class HashingServiceFactory : IHashingServiceFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<HashingServiceFactory> _logger;

        public HashingServiceFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<HashingServiceFactory>();
        }

        public async Task<IHashingService> CreateAsync(ISecretRepository secretRepository, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Creating {service} using hashing key in {secretRepository}", typeof(HashingService), secretRepository.GetType());
            using var hashingKey = await secretRepository.GetSecretAsync(Keys.HashingKey, stoppingToken);
            var hashingAlgorithm = new HMACSHA256(Encoding.UTF8.GetBytes(hashingKey.ToSystemString()));

            return new HashingService(hashingAlgorithm, _loggerFactory.CreateLogger<HashingService>());
        }
    }
}