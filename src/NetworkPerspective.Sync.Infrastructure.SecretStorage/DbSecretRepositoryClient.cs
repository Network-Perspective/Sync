using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    public class DbDataProtectionConfig
    {
        public string PublicKeyPath { get; set; }
        public string PrivateKeyPath { get; set; }
        public string SecretsPath { get; set; }
    }

    internal class DbSecretRepositoryClient : ISecretRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<DbDataProtectionConfig> _config;

        public DbSecretRepositoryClient(IUnitOfWorkFactory unitOfWorkFactory, IOptions<DbDataProtectionConfig> config)
        {
            _unitOfWork = unitOfWorkFactory.Create();
            _config = config;
        }

        public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
        {
            try
            {
                // return file contents if exits
                // this path is for hashing-key, google-key, and other connector dependent secrets
                var fn = Path.Join(_config.Value.SecretsPath, key);
                if (File.Exists(fn))
                {
                    return File.ReadAllText(fn).ToSecureString();
                }

                // else try to lookup key in database
                var secret = await _unitOfWork.GetDbSecretRepository().GetSecretAsync(key);

                return Decrypt(secret);
            }
            catch (Exception ex)
            {
                var message = $"Unable to get '{key}' from db secret repository. Please see inner exception";
                throw new SecretStorageException(message, ex);
            }
        }

        public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
        {
            try
            {
                await _unitOfWork.GetDbSecretRepository().RemoveSecretAsync(key, stoppingToken);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                var message = $"Unable to remove '{key}' from db secret repository. Please see inner exception";
                throw new SecretStorageException(message, ex);
            }
        }

        public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
        {
            try
            {
                var encrypted = Encrypt(secret);
                await _unitOfWork.GetDbSecretRepository().SetSecretAsync(key, encrypted);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                var message = $"Unable to set '{key}' to db secret repository. Please see inner exception";
                throw new SecretStorageException(message, ex);
            }
        }

        public string Encrypt(SecureString plainText)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(_config.Value.PublicKeyPath));

            var data = Encoding.UTF8.GetBytes(plainText.ToSystemString());
            var encryptedData = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);

            return Convert.ToBase64String(encryptedData);
        }

        public SecureString Decrypt(string cipherText)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(_config.Value.PrivateKeyPath));

            var encryptedData = Convert.FromBase64String(cipherText);
            var decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);

            return Encoding.UTF8.GetString(decryptedData).ToSecureString();
        }
    }
}