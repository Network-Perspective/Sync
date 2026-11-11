using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.Token;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.Ovh;

internal class OvhVaultClient : IVault
{
    private readonly OvhVaultConfig _config;
    private readonly ILogger<OvhVaultClient> _logger;
    private readonly IVaultClient _client;

    public OvhVaultClient(IOptions<OvhVaultConfig> config, ILogger<OvhVaultClient> logger)
        : this(config, logger, null)
    { }

    internal OvhVaultClient(IOptions<OvhVaultConfig> config, ILogger<OvhVaultClient> logger, IVaultClient client)
    {
        _config = config.Value;
        _logger = logger;
        _client = client;
    }

    private IVaultClient CreateVaultClient() => _client ?? new VaultClient(CreateClientSettings(_config));

    internal static VaultClientSettings CreateClientSettings(OvhVaultConfig config)
    {
        if (!new OvhVaultConfig.Validator("OvhVault").Validate(config).IsValid)
            throw new InvalidOperationException("Invalid OVH KV2 configuration.");

        // VaultSharp resolves relative v1/... paths; retain the OVH domain segment as a directory.
        var settings = new VaultClientSettings(config.BaseUrl.TrimEnd('/') + "/", new TokenAuthMethodInfo(config.Token));
        // Never forward the runtime token through an HTTP redirect.
        settings.PostProcessHttpClientHandlerAction = handler =>
        {
            if (handler is HttpClientHandler http)
                http.AllowAutoRedirect = false;
            else if (handler is SocketsHttpHandler sockets)
                sockets.AllowAutoRedirect = false;
            else
                throw new InvalidOperationException("Unsupported OVH KV2 HTTP handler.");
        };
        return settings;
    }

    public async Task<SecureString> GetSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        ValidateRequest(key, stoppingToken);
        try
        {
            _logger.LogDebug("Getting key '{key}' from OVH vault at {url}", key, _config.BaseUrl);
            var secret = await CreateVaultClient().V1.Secrets.KeyValue.V2.ReadSecretAsync(key, mountPoint: _config.MountPoint);
            var secureString = secret.Data.Data["secret"].ToString().ToSecureString();
            _logger.LogDebug("Got key '{key}' from OVH vault at {url}", key, _config.BaseUrl);
            return secureString;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw Failure($"Unable to get '{key}' from OVH vault at '{_config.BaseUrl}'.", ex);
        }
    }

    public async Task SetSecretAsync(string key, SecureString secret, CancellationToken stoppingToken = default)
    {
        ValidateRequest(key, stoppingToken);
        var data = new { secret = secret.ToSystemString() };
        // Bound the complete UTF-8 JSON envelope, not characters or the unencoded cache.
        if (JsonSerializer.SerializeToUtf8Bytes(new { data, options = new { } }).Length > 64000)
            throw new VaultException("OVH KV2 payload exceeds the conservative 64,000-byte request limit.");

        try
        {
            _logger.LogDebug("Setting key '{key}' to OVH vault at {url}", key, _config.BaseUrl);
            // shortcut: assumes one writer per secret; revisit CAS if ownership becomes shared.
            await CreateVaultClient().V1.Secrets.KeyValue.V2.WriteSecretAsync(key, data, mountPoint: _config.MountPoint);
            _logger.LogDebug("Set key '{key}' to OVH vault at {url}", key, _config.BaseUrl);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw Failure($"Unable to set '{key}' to OVH vault at '{_config.BaseUrl}'.", ex);
        }
    }

    public async Task RemoveSecretAsync(string key, CancellationToken stoppingToken = default)
    {
        ValidateRequest(key, stoppingToken);
        try
        {
            _logger.LogDebug("Removing key '{key}' from OVH vault at {url}", key, _config.BaseUrl);
            await CreateVaultClient().V1.Secrets.KeyValue.V2.DeleteSecretAsync(key, mountPoint: _config.MountPoint);
            _logger.LogDebug("Removed key '{key}' from OVH vault at {url}", key, _config.BaseUrl);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw Failure($"Unable to remove '{key}' from OVH vault at '{_config.BaseUrl}'.", ex);
        }
    }

    private static void ValidateRequest(string key, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(key) || key.Split('/').Any(segment =>
            segment.Length == 0 || segment == "." || segment == ".." ||
            segment.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_' && c != '.')))
            throw new ArgumentException("OVH KV2 keys must contain safe, nonempty path segments.", nameof(key));
    }

    private static VaultException Failure(string message, Exception exception)
    {
        // Provider errors may echo secret payloads. Never attach their body to logs/exceptions.
        return new VaultException(message + (exception is VaultApiException api ? $" HTTP {(int)api.HttpStatusCode}. No automatic retry." : string.Empty));
    }
}