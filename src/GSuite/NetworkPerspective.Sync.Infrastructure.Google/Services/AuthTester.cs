using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal class AuthTester : IAuthTester
    {
        private readonly GoogleConfig _config;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly INetworkService _networkService;
        private readonly INetworkIdProvider _networkIdProvider;
        private readonly ILogger<AuthTester> _logger;

        public AuthTester(IOptions<GoogleConfig> config, ICredentialsProvider credentialsProvider, INetworkService networkService, INetworkIdProvider networkIdProvider, ILogger<AuthTester> logger)
        {
            _config = config.Value;
            _credentialsProvider = credentialsProvider;
            _networkService = networkService;
            _networkIdProvider = networkIdProvider;
            _logger = logger;
        }

        public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
        {
            var networkId = _networkIdProvider.Get();
            try
            {
                _logger.LogInformation("Checking if network '{networkId}' is authorized", networkId);
                var network = await _networkService.GetAsync<GoogleNetworkProperties>(networkId, stoppingToken);

                var userCredentials = await _credentialsProvider.GetForUserAsync(network.Properties.AdminEmail, stoppingToken);

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