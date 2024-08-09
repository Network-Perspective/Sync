using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager;

internal class GoogleSecretManagerClient : IVault
{
    private readonly GoogleSecretManagerConfig _config;
    private readonly ILogger<GoogleSecretManagerClient> _logger;
    private readonly SecretManagerServiceClient _client;

    public GoogleSecretManagerClient(IOptions<GoogleSecretManagerConfig> config, ILogger<GoogleSecretManagerClient> logger)
    {
        _logger = logger;
        _config = config.Value;
        _client = SecretManagerServiceClient.Create();
    }

    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Getting key '{key}' from Google Secret Manager for project '{projectId}'", key, _config.ProjectId);
            var secretVersionName = new SecretVersionName(_config.ProjectId, key, "latest");
            var result = await _client.AccessSecretVersionAsync(secretVersionName, stoppingToken);
            _logger.LogDebug("Got key '{key}' from Google Secret Manager for project '{projectId}'", key, _config.ProjectId);
            return result.Payload.Data.ToStringUtf8().ToSecureString();
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{key}' from Google Secret Manager for project '{_config.ProjectId}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Removing key '{key}' from Google Secret Manager for project '{projectId}'", key, _config.ProjectId);

            var secretName = new SecretName(_config.ProjectId, key);
            await _client.DeleteSecretAsync(secretName, stoppingToken);
            _logger.LogDebug("Removed key '{key}' from Google Secret Manager for project '{projectId}'", key, _config.ProjectId);
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{key}' from Google Secret Manager for project '{_config.ProjectId}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Setting key '{key}' to Google Secret Manager for project '{projectId}'", key, _config.ProjectId);

            var secretName = new SecretName(_config.ProjectId, key);

            if (!await SecretExistsAsync(secretName))
            {
                _logger.LogDebug("Key '{key}' is not yet initialized in Google Secret Manager for project '{projectId}'", key, _config.ProjectId);
                var projectName = new ProjectName(_config.ProjectId);
                var gSecret = new Secret
                {
                    Replication = new Replication
                    {
                        Automatic = new Replication.Types.Automatic(),
                    }
                };
                await _client.CreateSecretAsync(projectName, key, gSecret);
            }

            var secretPayload = new SecretPayload
            {
                Data = ByteString.CopyFromUtf8(secret.ToSystemString())
            };
            await _client.AddSecretVersionAsync(secretName, secretPayload);
            _logger.LogDebug("Set key '{key}' to Google Secret Manager for project '{projectId}'", key, _config.ProjectId);
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{key}' from Google Secret Manager for project '{_config.ProjectId}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    private async Task<bool> SecretExistsAsync(SecretName secretName)
    {
        try
        {
            await _client.GetSecretAsync(secretName);
            return true;
        }
        catch (Grpc.Core.RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return false;
        }
    }
}