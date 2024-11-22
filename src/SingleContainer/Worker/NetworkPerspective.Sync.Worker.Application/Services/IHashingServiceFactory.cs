using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IHashingServiceFactory
{
    Task<IHashingService> CreateAsync(CancellationToken stoppingToken = default);
}

internal sealed class HashingServiceFactory(IVault vault, ILoggerFactory loggerFactory) : IHashingServiceFactory
{
    private readonly ILogger<HashingServiceFactory> _logger = loggerFactory.CreateLogger<HashingServiceFactory>();

    public async Task<IHashingService> CreateAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Creating {service} using hashing key in {secretRepository}", typeof(HashingService), vault.GetType());
        using var hashingKey = await vault.GetSecretAsync(Keys.HashingKey, stoppingToken);
        var hashingAlgorithm = new HMACSHA256(Encoding.UTF8.GetBytes(hashingKey.ToSystemString()));

        return new HashingService(hashingAlgorithm, loggerFactory.CreateLogger<HashingService>());
    }
}