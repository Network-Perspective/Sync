using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services
{
    internal class AuthTester : IAuthTester
    {
        private readonly GoogleConfig _config;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly IConnectorService _connectorService;
        private readonly IConnectorInfoProvider _connectorInfoProvider;
        private readonly ILogger<AuthTester> _logger;

        public AuthTester(IOptions<GoogleConfig> config, ICredentialsProvider credentialsProvider, IConnectorService connectorService, IConnectorInfoProvider connectorInfoProvider, ILogger<AuthTester> logger)
        {
            _config = config.Value;
            _credentialsProvider = credentialsProvider;
            _connectorService = connectorService;
            _connectorInfoProvider = connectorInfoProvider;
            _logger = logger;
        }

        public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
        {
            var connectorInfo = _connectorInfoProvider.Get();
            try
            {
                _logger.LogInformation("Checking if connector '{connectorId}' is authorized", connectorInfo);
                var network = await _connectorService.GetAsync<GoogleNetworkProperties>(connectorInfo.Id, stoppingToken);

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
                _logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorInfo);
                return false;
            }
        }
    }
}