using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AmazonSecretsManager;

internal class AmazonSecretsManagerClient : IVault
{
    private const char SecretKeySeparator = '/';
    private readonly AmazonSecretsManagerConfig _config;
    private readonly ILogger<AmazonSecretsManagerClient> _logger;
    private readonly Lazy<IAmazonSecretsManager> _client;

    public AmazonSecretsManagerClient(IOptions<AmazonSecretsManagerConfig> config, ILogger<AmazonSecretsManagerClient> logger)
    {
        _config = config.Value;
        _logger = logger;
        _client = new Lazy<IAmazonSecretsManager>(() =>
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(config.Value.Region);
            return new Amazon.SecretsManager.AmazonSecretsManagerClient(regionEndpoint);
        });
    }

    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        var secretId = GetAmazonSecretId(key);

        try
        {
            _logger.LogDebug("Getting key '{key}' from Amazon Secrets Manager for region '{region}'", secretId, _config.Region);
            var reqest = new GetSecretValueRequest { SecretId = secretId };
            var result = await _client.Value.GetSecretValueAsync(reqest, stoppingToken);
            _logger.LogDebug("Got key '{key}' from Amazon Secrets Manager for region '{region}'", key, _config.Region);
            return result.SecretString.ToSecureString();
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{secretId}' from Amazon Secrets Manager for region '{_config.Region}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        var secretId = GetAmazonSecretId(key);

        try
        {
            _logger.LogDebug("Removing key '{key}' from Amazon Secrets Manager for region '{region}'", secretId, _config.Region);
            var request = new DeleteSecretRequest { SecretId = secretId };
            await _client.Value.DeleteSecretAsync(request, stoppingToken);
            _logger.LogDebug("Removed key '{key}' from Amazon Secrets Manager for region '{region}'", secretId, _config.Region);
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{secretId}' from Amazon Secrets Manager for region '{_config.Region}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
    {
        var secretId = GetAmazonSecretId(key);

        try
        {
            _logger.LogDebug("Setting key '{key}' to Amazon Secrets Manager for region '{region}'", secretId, _config.Region);

            if (!await SecretExistsAsync(secretId, stoppingToken))
            {
                _logger.LogDebug("Key '{key}' is not yet initialized in Amazon Secrets Manager for region '{projectId}'", secretId, _config.Region);
                var createSecretRequest = new CreateSecretRequest
                {
                    Name = secretId,
                    SecretString = secret.ToSystemString()
                };
                await _client.Value.CreateSecretAsync(createSecretRequest, stoppingToken);
                return;
            }
            else
            {
                _logger.LogDebug("Key '{key}' is already initialized in Amazon Secrets Manager for region '{projectId}'", secretId, _config.Region);
                var request = new UpdateSecretRequest
                {
                    SecretId = secretId,
                    SecretString = secret.ToSystemString()
                };
                await _client.Value.UpdateSecretAsync(request, stoppingToken);
            }

            _logger.LogDebug("Set key '{key}' to Amazon Secrets Manager for region '{region}'", secretId, _config.Region);
        }
        catch (Exception ex)
        {
            var message = $"Unable to set '{secretId}' to Amazon Secrets Manager for region '{_config.Region}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    private string GetAmazonSecretId(string key)
        => string.Join(SecretKeySeparator, _config.SecretsPrefix.TrimEnd([SecretKeySeparator]), key);

    private async Task<bool> SecretExistsAsync(string secretId, CancellationToken stoppingToken)
    {
        try
        {
            var request = new GetSecretValueRequest { SecretId = secretId };
            await _client.Value.GetSecretValueAsync(request, stoppingToken);
            return true;
        }
        catch (ResourceNotFoundException)
        {
            return false;
        }
    }
}