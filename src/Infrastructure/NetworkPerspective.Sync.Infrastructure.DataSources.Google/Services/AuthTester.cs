using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal class AuthTester(IOptions<GoogleConfig> config, ICredentialsProvider credentialsProvider, IConnectorInfoProvider connectorInfoProvider, ILogger<AuthTester> logger) : IAuthTester
{
    private readonly GoogleConfig _config = config.Value;

    public async Task<bool> IsAuthorizedAsync(IDictionary<string, string> networkProperties, CancellationToken stoppingToken = default)
    {
        var connectorInfo = connectorInfoProvider.Get();
        try
        {
            logger.LogInformation("Checking if connector '{connectorId}' is authorized", connectorInfo);
            var googleNetworkProperties = ConnectorProperties.Create<GoogleNetworkProperties>(networkProperties);

            var userCredentials = await credentialsProvider.GetForUserAsync(googleNetworkProperties.AdminEmail, stoppingToken);

            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = _config.ApplicationName,
            });

            var request = service.Users.List();
            request.MaxResults = 10;
            request.Domain = googleNetworkProperties.Domain;
            var response = await request.ExecuteAsync();
            return response.UsersValue != null;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorInfo);
            return false;
        }
    }
}