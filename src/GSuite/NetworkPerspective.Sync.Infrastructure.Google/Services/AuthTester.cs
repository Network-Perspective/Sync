using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal class AuthTester : IAuthTester
    {
        private readonly GoogleConfig _config;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly INetworkService _networkService;
        private readonly ILogger<AuthTester> _logger;

        public AuthTester(IOptions<GoogleConfig> config, ISecretRepositoryFactory secretRepositoryFactory, INetworkService networkService, ILogger<AuthTester> logger)
        {
            _config = config.Value;
            _secretRepositoryFactory = secretRepositoryFactory;
            _networkService = networkService;
            _logger = logger;
        }

        public async Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);
                var network = await _networkService.GetAsync<GoogleNetworkProperties>(networkId, stoppingToken);

                var secretRepository = await _secretRepositoryFactory.CreateAsync(networkId, stoppingToken);
                var credentials = await new CredentialsProvider(secretRepository).GetCredentialsAsync(stoppingToken);

                var userCredentials = credentials
                    .CreateWithUser(network.Properties.AdminEmail)
                    .UnderlyingCredential as ServiceAccountCredential;

                var service = new DirectoryService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredentials,
                    ApplicationName = _config.ApplicationName,
                });

                var request = service.Users.List();
                request.MaxResults = 10;
                request.Domain = network.Properties.Domain;
                var response = await request.ExecuteAsync();
                return response.UsersValue != null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Network '{networkId}' is not authorized", networkId);
                return false;
            }
        }
    }
}