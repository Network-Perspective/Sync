using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;

internal interface ICredentialsService
{
    Task<ICredential> ImpersonificateAsync(string email, CancellationToken stoppingToken = default);
    Task<ICredential> GetUserCredentialsAsync(CancellationToken stoppingToken = default);
    Task TryRefreshUserAccessTokenAsync(CancellationToken stoppingToken = default);
    Task<string> GetServiceAccountClientIdAsync(CancellationToken stoppingToken = default);
}

internal class CredentialsService(IImpesonificationCredentialsProvider impesonificationCredentialsProvider, IUserCredentialsService userCredentialsProvider, IStatusLogger statusLogger, ILogger<CredentialsService> logger) : ICredentialsService
{
    public Task<ICredential> GetUserCredentialsAsync(CancellationToken stoppingToken = default)
        => userCredentialsProvider.GetCurrentAsync(stoppingToken);

    public Task<ICredential> ImpersonificateAsync(string email, CancellationToken stoppingToken = default)
        => impesonificationCredentialsProvider.ImpersonificateAsync(email, stoppingToken);

    public async Task TryRefreshUserAccessTokenAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            await userCredentialsProvider.RefreshTokenAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to refresh user token");
            await statusLogger.LogWarningAsync("Unable to refresh token. Please try to re-authenticate", stoppingToken);
        }
    }

    public Task<string> GetServiceAccountClientIdAsync(CancellationToken stoppingToken = default)
        => impesonificationCredentialsProvider.GetClientIdAsync(stoppingToken);
}