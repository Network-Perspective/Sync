using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Gmail.v1;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    public interface ICredentialsProvider
    {
        Task<GoogleCredential> GetCredentialsAsync(CancellationToken stoppingToken = default);
    }

    internal sealed class CredentialsProvider : ICredentialsProvider
    {
        private static readonly string[] Scopes =
        {
            GmailService.Scope.GmailMetadata,
            DirectoryService.Scope.AdminDirectoryUserReadonly,
            CalendarService.Scope.CalendarReadonly
        };

        private readonly ISecretRepository _secretRepository;

        public CredentialsProvider(ISecretRepository secretRepository)
        {
            _secretRepository = secretRepository;
        }

        public async Task<GoogleCredential> GetCredentialsAsync(CancellationToken stoppingToken = default)
        {
            var googleKey = await _secretRepository.GetSecretAsync(GoogleKeys.TokenKey, stoppingToken);

            return GoogleCredential
                .FromJson(new NetworkCredential(string.Empty, googleKey).Password)
                .CreateScoped(Scopes);
        }
    }
}