using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;

using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.AuthMethods.Token;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    public class HcpVaultClient : ISecretRepository
    {
        private readonly HcpVaultConfig _config;
        private readonly ILogger<HcpVaultClient> _logger;

        public HcpVaultClient(IOptions<HcpVaultConfig> config, ILogger<HcpVaultClient> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        private VaultClient CreateVaultClient()
        {
            IAuthMethodInfo authMethod;
            if (!string.IsNullOrEmpty(_config.Token))
            {
                authMethod = new TokenAuthMethodInfo(_config.Token);
            }
            else
            {
                // Read the JWT token for the service account
                var jwt = File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
                authMethod = new KubernetesAuthMethodInfo(_config.VaultRole, jwt);
            }

            // Initialize settings. You can also set proxies, custom delegates etc. here.
            var vaultClientSettings = new VaultClientSettings(_config.BaseUrl, authMethod);

            return new VaultClient(vaultClientSettings);
        }

        public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Getting key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
                var secret = await CreateVaultClient().V1.Secrets.KeyValue.V2.ReadSecretAsync(key, mountPoint: _config.MountPoint);
                var secureString = secret.Data.Data["secret"].ToString().ToSecureString();
                _logger.LogDebug("Got key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
                return secureString;
            }
            catch (Exception ex)
            {
                var message = $"Unable to get '{key}' from internal key vault at '{_config.BaseUrl}'. Please see inner exception";
                _logger.LogDebug(ex, message);
                throw new SecretStorageException(message, ex);
            }
        }

        public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Setting key '{key}' to internal key vault at {url}", key, _config.BaseUrl);
                await CreateVaultClient().V1.Secrets.KeyValue.V2.WriteSecretAsync(key, new { secret = secret.ToSystemString() }, mountPoint: _config.MountPoint);
                _logger.LogDebug("Set key '{key}' to internal key vault at {url}", key, _config.BaseUrl);
            }
            catch (Exception ex)
            {
                var message = $"Unable to set '{key}' to internal key vault at '{_config.BaseUrl}'. Please see inner exception";
                throw new SecretStorageException(message, ex);
            }
        }

        public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Removing key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
                await CreateVaultClient().V1.Secrets.KeyValue.V2.DeleteSecretAsync(key, mountPoint: _config.MountPoint);
                _logger.LogDebug("Removed key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
            }
            catch (Exception ex)
            {
                var message = $"Unable to remove '{key}' from internal key vault at '{_config.BaseUrl}'. Please see inner exception";
                throw new SecretStorageException(message, ex);
            }
        }
    }
}