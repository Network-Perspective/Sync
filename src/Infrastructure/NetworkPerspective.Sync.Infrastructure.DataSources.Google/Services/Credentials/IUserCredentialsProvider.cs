using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;

internal interface IUserCredentialsProvider
{
    Task<ICredential> GetCurrentAsync(CancellationToken stoppingToken = default);
}

internal class UserCredentialsProvider(IVault vault, IConnectorContextAccessor connectorContextAccessor) : IUserCredentialsProvider
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
}