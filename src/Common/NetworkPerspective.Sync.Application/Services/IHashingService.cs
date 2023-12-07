using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IHashingService : IDisposable
    {
        string Hash(string input);
    }

    internal class HashingService : IHashingService
    {
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly ILogger<HashingService> _logger;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public HashingService(HashAlgorithm hashAlgorithm, ILogger<HashingService> logger)
        {
            _hashAlgorithm = hashAlgorithm;
            _logger = logger;
        }

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
                if (input == null)
                {
                    _logger.LogTrace("Skipping hashing null object");
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
}