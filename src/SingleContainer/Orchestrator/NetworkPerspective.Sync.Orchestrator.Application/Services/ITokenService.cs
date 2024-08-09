using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface ITokenService
{
    Task AddOrReplace(SecureString accessToken, Guid connectorId, CancellationToken stoppingToken = default);
    Task<SecureString> GetAsync(Guid connectorId, CancellationToken stoppingToken = default);
    Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingToken = default);
}

internal class TokenService : ITokenService
{
    private readonly IVault _vault;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IVault vault, ILogger<TokenService> logger)
    {
        _vault = vault;
        _logger = logger;
    }

    public async Task AddOrReplace(SecureString accessToken, Guid connectorId, CancellationToken stoppingToken = default)
    {
        _logger.LogDebug("Saving Access Token for connector '{connectorId}'...", connectorId);

        var tokenKey = GetAccessTokenKey(connectorId);
        await _vault.SetSecretAsync(tokenKey, accessToken, stoppingToken);

        _logger.LogDebug("Access Token for connector '{connectorId}' saved", connectorId);
    }

    public async Task<SecureString> GetAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        var tokenKey = GetAccessTokenKey(connectorId);
        return await _vault.GetSecretAsync(tokenKey, stoppingToken);
    }

    public async Task EnsureRemovedAsync(Guid connectorId, CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogDebug("Removing Access Token for connector '{connectorId}'...", connectorId);
            var tokenKey = GetAccessTokenKey(connectorId);
            await _vault.RemoveSecretAsync(tokenKey, stoppingToken);
            _logger.LogDebug("Removed Access Token for connector '{connectorId}'...", connectorId);
        }
        catch (Exception)
        {
            _logger.LogDebug("Unable to remove Access Token for connector '{connectorId}', maybe there is nothing to remove?", connectorId);
        }
    }

    private static string GetAccessTokenKey(Guid connectorId)
        => string.Format(Keys.TokenKeyPattern, connectorId.ToString());
}