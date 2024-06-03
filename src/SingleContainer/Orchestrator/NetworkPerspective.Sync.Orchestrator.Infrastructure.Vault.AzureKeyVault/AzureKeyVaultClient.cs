using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.AzureKeyVault;

internal class AzureKeyVaultClient : IVault
{
    private readonly AzureKeyVaultConfig _config;
    private readonly TokenCredential _tokenCredential;
    private readonly ILogger<AzureKeyVaultClient> _logger;

    public AzureKeyVaultClient(TokenCredential tokenCredential, IOptions<AzureKeyVaultConfig> config, ILogger<AzureKeyVaultClient> logger)
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
            throw new VaultException(message, ex);
        }
    }
}