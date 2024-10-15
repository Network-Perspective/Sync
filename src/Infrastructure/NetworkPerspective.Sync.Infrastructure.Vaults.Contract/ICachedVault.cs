using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

public interface ICachedVault : IVault
{
    Task ClearCacheAsync(CancellationToken stoppingToken = default);
}

public class CachedVault(IVault secretRepository, ILogger<CachedVault> logger) : ICachedVault
{
    private readonly Dictionary<string, SecureString> _cachedSecrets = [];
    private readonly SemaphoreSlim _semaphore = new(1);

    public async Task ClearCacheAsync(CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);
        try
        {
            logger.LogDebug("Clearing cache...");
            _cachedSecrets.Clear();
            logger.LogDebug("Cache cleared");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);
        try
        {
            if (!_cachedSecrets.ContainsKey(key))
            {
                logger.LogDebug("Initializing key '{key}' in cache...", key);
                _cachedSecrets[key] = await secretRepository.GetSecretAsync(key, stoppingToken);
            }

            return _cachedSecrets[key];
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
        => secretRepository.RemoveSecretAsync(key, stoppingToken);

    public Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
        => secretRepository.SetSecretAsync(key, secret, stoppingToken);
}