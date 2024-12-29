using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;

internal interface ICredentialsProvider
{
    Task<ServiceAccountCredential> ImpersonificateAsync(string email, CancellationToken stoppingToken = default);
    Task<GoogleCredential> GetCurrentAsync(CancellationToken stoppingToken = default);
}

internal class CredentialsProvider(IImpesonificationCredentialsProvider impesonificationCredentialsProvider, IUserCredentialsProvider userCredentialsProvider) : ICredentialsProvider
{
    public Task<GoogleCredential> GetCurrentAsync(CancellationToken stoppingToken = default)
        => userCredentialsProvider.GetCurrentAsync(stoppingToken);

    public Task<ServiceAccountCredential> ImpersonificateAsync(string email, CancellationToken stoppingToken = default)
        => impesonificationCredentialsProvider.ImpersonificateAsync(email, stoppingToken);
}