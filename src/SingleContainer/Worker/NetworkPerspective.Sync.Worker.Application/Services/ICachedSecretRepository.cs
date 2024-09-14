using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ICachedSecretRepository : IVault
{
    Task ClearCacheAsync(CancellationToken stoppingToken = default);
}

internal class CachedSecretRepository : ICachedSecretRepository
{
    private readonly IVault _secretRepository;
    private readonly ILogger<CachedSecretRepository> _logger;
    private readonly IDictionary<string, SecureString> _cachedSecrets = new Dictionary<string, SecureString>();
    private readonly SemaphoreSlim _semaphore = new(1);

    public CachedSecretRepository(IVault secretRepository, ILogger<CachedSecretRepository> logger)
    {
        _secretRepository = secretRepository;
        _logger = logger;
    }

    public async Task ClearCacheAsync(CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);
        try
        {
            _logger.LogDebug("Clearing cache...");
            _cachedSecrets.Clear();
            _logger.LogDebug("Cache cleared");
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
                _logger.LogDebug("Initializing key '{key}' in cache...", key);
                _cachedSecrets[key] = await _secretRepository.GetSecretAsync(key, stoppingToken);
            }

            return _cachedSecrets[key];
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
        => _secretRepository.RemoveSecretAsync(key, stoppingToken);

    public Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
        => _secretRepository.SetSecretAsync(key, secret, stoppingToken);
}