﻿using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault;

internal class ExternalAzureKeyVaultClient : IVault
{
    private const string AppTenantIdKeyName = "app-tenant-id";
    private const string AppClientIdKeyName = "app-client-id";
    private const string AppClientSecretKeyName = "app-client-secret";

    private readonly IVault _internalSecretRepository;
    private readonly ExternalAzureKeyVaultConfig _config;
    private readonly ILogger<ExternalAzureKeyVaultClient> _logger;
    private readonly Lazy<Task<ClientSecretCredential>> _tokenCredential;
    public ExternalAzureKeyVaultClient(IVault internalSecretRepository, IOptions<ExternalAzureKeyVaultConfig> config, ILogger<ExternalAzureKeyVaultClient> logger)
    {
        _internalSecretRepository = internalSecretRepository;
        _config = config.Value;
        _logger = logger;
        _tokenCredential = new Lazy<Task<ClientSecretCredential>>(InitializeClientSecretCredentialAsync);
    }

    private async Task<ClientSecretCredential> InitializeClientSecretCredentialAsync()
    {
        var tenantId = await _internalSecretRepository.GetSecretAsync(AppTenantIdKeyName);
        var clientId = await _internalSecretRepository.GetSecretAsync(AppClientIdKeyName);
        var clientSecret = await _internalSecretRepository.GetSecretAsync(AppClientSecretKeyName);

        var clientSecretCredentialOptions = new ClientSecretCredentialOptions();
        clientSecretCredentialOptions.AdditionallyAllowedTenants.Add("*");

        return new ClientSecretCredential(
            tenantId.ToSystemString(),
            clientId.ToSystemString(),
            clientSecret.ToSystemString(),
            clientSecretCredentialOptions);
    }

    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Getting key '{key}' from external key vault at {url}", key, _config.BaseUrl);
            var keyVaultClient = new SecretClient(new Uri(_config.BaseUrl), await _tokenCredential.Value);
            var secret = await keyVaultClient.GetSecretAsync(key, string.Empty, stoppingToken);
            var secureString = secret.Value.Value.ToSecureString();
            _logger.LogDebug("Got key '{key}' from external key vault at {url}", key, _config.BaseUrl);
            return secureString;
        }
        catch (Exception ex)
        {
            var message = $"Unable to get '{key}' from external key vault at '{_config.BaseUrl}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Setting key '{key}' to external key vault at {url}", key, _config.BaseUrl);
            var keyVaultClient = new SecretClient(new Uri(_config.BaseUrl), await _tokenCredential.Value);
            var vaultSecret = new KeyVaultSecret(key, secret.ToSystemString());
            await keyVaultClient.SetSecretAsync(vaultSecret, stoppingToken);
            _logger.LogDebug("Set key '{key}' to external key vault at {url}", key, _config.BaseUrl);
        }
        catch (Exception ex)
        {
            var message = $"Unable to set '{key}' to external key vault at '{_config.BaseUrl}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }

    public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Removing key '{key}' from external key vault at {url}", key, _config.BaseUrl);
            var keyVaultClient = new SecretClient(new Uri(_config.BaseUrl), await _tokenCredential.Value);
            var deleteOperation = await keyVaultClient.StartDeleteSecretAsync(key, stoppingToken);
            await deleteOperation.WaitForCompletionAsync(stoppingToken);
            var response = keyVaultClient.PurgeDeletedSecretAsync(key, stoppingToken);
            _logger.LogDebug("Removed key '{key}' from external key vault at {url}", key, _config.BaseUrl);

        }
        catch (Exception ex)
        {
            var message = $"Unable to remove '{key}' from external key vault at '{_config.BaseUrl}'. Please see inner exception";
            throw new VaultException(message, ex);
        }
    }
}