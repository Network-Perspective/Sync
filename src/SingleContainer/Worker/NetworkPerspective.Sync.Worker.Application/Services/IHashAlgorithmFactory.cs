using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IHashAlgorithmFactory
{
    Task<HashAlgorithm> CreateAsync(CancellationToken stoppingToken = default);
}

internal sealed class HashAlgorithmFactory(IVault vault, ILogger<HashAlgorithmFactory> logger) : IHashAlgorithmFactory
{
    public async Task<HashAlgorithm> CreateAsync(CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Creating hashing algorithm using hashing key in {secretRepository}", vault.GetType());
        using var hashingKey = await vault.GetSecretAsync(Keys.HashingKey, stoppingToken);
        var hashingAlgorithm = new HMACSHA256(Encoding.UTF8.GetBytes(hashingKey.ToSystemString()));

        return hashingAlgorithm;
    }
}