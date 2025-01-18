using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IHashingService
{
    string Hash(string input);
}

internal sealed class HashingService(IHashAlgorithmFactory hashAlgorithmFactory, ILogger<HashingService> logger) : IHashingService, IDisposable
{
    private HashAlgorithm _hashAlgorithm;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public void Dispose()
    {
        _hashAlgorithm?.Dispose();
        _semaphoreSlim?.Dispose();
    }

    public string Hash(string input)
    {
        try
        {
            _semaphoreSlim.Wait();

            _hashAlgorithm ??= hashAlgorithmFactory.CreateAsync().Result;

            if (input == null)
            {
                logger.LogTrace("Skipping hashing null object");
                return null;
            }

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashedBytes = _hashAlgorithm.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashedBytes);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}