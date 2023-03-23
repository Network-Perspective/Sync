using System;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;

using Microsoft.Graph;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    public interface IMicrosoftClientFactory
    {
        Task<GraphServiceClient> GetMicrosoftClientAsync(Guid networkId, CancellationToken stoppingToken = default);
    }

    public class MicrosoftClientFactory : IMicrosoftClientFactory
    {
        private readonly string[] Scopes = new[] { "https://graph.microsoft.com/.default" };

        private readonly ISecretRepositoryFactory _secretRepositoryFactory;

        public MicrosoftClientFactory(ISecretRepositoryFactory secretRepositoryFactory)
        {
            _secretRepositoryFactory = secretRepositoryFactory;
        }

        public async Task<GraphServiceClient> GetMicrosoftClientAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);

            var tenantIdKey = string.Format(MicrosoftKeys.MicrosoftTenantIdPattern, networkId);
            var tenantId = await secretRepository.GetSecretAsync(tenantIdKey, stoppingToken);

            var clientIdKey = string.Format(MicrosoftKeys.MicrosoftClientIdPattern, networkId);
            var clientId = await secretRepository.GetSecretAsync(clientIdKey, stoppingToken);

            var clientSecretKey = string.Format(MicrosoftKeys.MicrosoftClientSecretPattern, networkId);
            var clientSecret = await secretRepository.GetSecretAsync(clientSecretKey, stoppingToken);

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId.ToSystemString(), clientId.ToSystemString(), clientSecret.ToSystemString(), options);

            return new GraphServiceClient(clientSecretCredential, Scopes);
        }
    }
}