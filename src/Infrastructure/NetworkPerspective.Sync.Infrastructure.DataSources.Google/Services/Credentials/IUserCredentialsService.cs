using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Clients;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;

internal interface IUserCredentialsService
{
    Task<ICredential> GetCurrentAsync(CancellationToken stoppingToken = default);
    Task RefreshTokenAsync(CancellationToken stoppingToken = default);
}

internal class UserCredentialsService(IVault vault, IOAuthClient oAuthClient, IConnectorContextAccessor connectorContextAccessor) : IUserCredentialsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private GoogleCredential _credential = null;


    public async Task<ICredential> GetCurrentAsync(CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);

        try
        {
            if (_credential is null)
            {
                var accessToken = await GetAccessTokenAsync(stoppingToken);
                _credential = GoogleCredential.FromAccessToken(accessToken.ToSystemString());
            }

            return _credential;
        }
        finally
        {
            _semaphore.Release();
        }
    }



    private async Task<SecureString> GetAccessTokenAsync(CancellationToken stoppingToken)
    {
        try
        {
            var connectorId = connectorContextAccessor.Context.ConnectorId;
            var accessTokenKey = string.Format(GoogleKeys.GoogleAccessTokenKeyPattern, connectorId);
            return await vault.GetSecretAsync(accessTokenKey, stoppingToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to get access token", ex);
        }
    }

    public async Task RefreshTokenAsync(CancellationToken stoppingToken = default)
    {
        var clientId = await vault.GetSecretAsync(GoogleKeys.GoogleClientIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(GoogleKeys.GoogleClientSecretKey, stoppingToken);

        var connectorId = connectorContextAccessor.Context.ConnectorId;

        var refreshTokenKey = string.Format(GoogleKeys.GoogleRefreshTokenPattern, connectorId);
        var refreshToken = await vault.GetSecretAsync(refreshTokenKey, stoppingToken);

        var response = await oAuthClient.RefreshTokenAsync(refreshToken.ToSystemString(), clientId.ToSystemString(), clientSecret.ToSystemString(), CancellationToken.None);

        await vault.SetSecretAsync(refreshTokenKey, response.RefreshToken.ToSecureString(), CancellationToken.None);

        var accessTokenKey = string.Format(GoogleKeys.GoogleAccessTokenKeyPattern, connectorId);
        await vault.SetSecretAsync(accessTokenKey, response.AccessToken.ToSecureString(), CancellationToken.None);
    }
}