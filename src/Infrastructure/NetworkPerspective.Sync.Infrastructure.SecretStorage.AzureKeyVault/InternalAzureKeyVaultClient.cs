using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.AzureKeyVault;

public class InternalAzureKeyVaultClient : ISecretRepository
{
    private readonly AzureKeyVaultConfig _config;
    private readonly TokenCredential _tokenCredential;
    private readonly ILogger<InternalAzureKeyVaultClient> _logger;

    public InternalAzureKeyVaultClient(TokenCredential tokenCredential, IOptions<AzureKeyVaultConfig> config, ILogger<InternalAzureKeyVaultClient> logger)
    {
        _config = config.Value;
        _tokenCredential = tokenCredential;
        _logger = logger;
    }

    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Getting key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
            var keyVaultClient = new SecretClient(new Uri(_config.BaseUrl), _tokenCredential);
            var secret = await keyVaultClient.GetSecretAsync(key, string.Empty, stoppingToken);
            var secureString = secret.Value.Value.ToSecureString();
            _logger.LogDebug("Got key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
            return secureString;
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{key}' from internal key vault at '{_config.BaseUrl}'. Please see inner exception";
            throw new SecretStorageException(message, ex);
        }
    }

    public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Setting key '{key}' to internal key vault at {url}", key, _config.BaseUrl);
            var keyVaultClient = new SecretClient(new Uri(_config.BaseUrl), _tokenCredential);
            var vaultSecret = new KeyVaultSecret(key, secret.ToSystemString());
            await keyVaultClient.SetSecretAsync(vaultSecret, stoppingToken);
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
            var keyVaultClient = new SecretClient(new Uri(_config.BaseUrl), _tokenCredential);
            var deleteOperation = await keyVaultClient.StartDeleteSecretAsync(key, stoppingToken);
            await deleteOperation.WaitForCompletionAsync(stoppingToken);
            var response = keyVaultClient.PurgeDeletedSecretAsync(key, stoppingToken);
            _logger.LogDebug("Removed key '{key}' from internal key vault at {url}", key, _config.BaseUrl);
        }
        catch (Exception ex)
        {
            var message = $"Unable to remove '{key}' from internal key vault at '{_config.BaseUrl}'. Please see inner exception";
            throw new SecretStorageException(message, ex);
        }
    }
}