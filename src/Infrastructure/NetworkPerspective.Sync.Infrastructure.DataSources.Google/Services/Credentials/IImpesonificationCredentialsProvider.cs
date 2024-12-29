using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Gmail.v1;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;

public interface IImpesonificationCredentialsProvider
{
    Task<ServiceAccountCredential> ImpersonificateAsync(string email, CancellationToken stoppingToken = default);
}

internal sealed class ImpersonificationCredentialsProvider(IVault vault) : IImpesonificationCredentialsProvider
{
    private static readonly string[] Scopes =
    [
        GmailService.Scope.GmailMetadata,
        DirectoryService.Scope.AdminDirectoryUserReadonly,
        CalendarService.Scope.CalendarReadonly
    ];

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private GoogleCredential _credential = null;

    public async Task<ServiceAccountCredential> ImpersonificateAsync(string email, CancellationToken stoppingToken = default)
    {
        var googleCredentials = await GetCredentialsAsync(stoppingToken);

        var userCredentials = googleCredentials
            .CreateWithUser(email)
            .UnderlyingCredential as ServiceAccountCredential;

        var handler = new UnsuccessfulResponseHandler(ClearCachedCredentialsAsync);

        userCredentials
            .HttpClient
            .MessageHandler
            .AddUnsuccessfulResponseHandler(handler);

        return userCredentials;
    }

    private async Task ClearCachedCredentialsAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await _semaphore.WaitAsync(cts.Token);
        try
        {
            _credential = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<GoogleCredential> GetCredentialsAsync(CancellationToken stoppingToken = default)
    {
        await _semaphore.WaitAsync(stoppingToken);

        try
        {
            if (_credential is null)
            {
                var googleKey = await vault.GetSecretAsync(GoogleKeys.TokenKey, stoppingToken);

                _credential = GoogleCredential
                    .FromJson(googleKey.ToSystemString())
                    .CreateScoped(Scopes);
            }

            return _credential;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}