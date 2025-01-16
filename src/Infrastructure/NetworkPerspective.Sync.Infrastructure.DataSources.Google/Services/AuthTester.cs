using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal class AuthTester(IOptions<GoogleConfig> config, IImpesonificationCredentialsProvider credentialsProvider, IConnectorContextAccessor connectorContextProvider, ILogger<AuthTester> logger) : IAuthTester
{
    private readonly GoogleConfig _config = config.Value;

    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        const string currentAccountCustomer = "my_customer";

        var connectorContext = connectorContextProvider.Context;
        try
        {
            logger.LogInformation("Checking if connector '{connectorId}' is authorized", connectorContext);
            var googleNetworkProperties = new GoogleConnectorProperties(connectorContext.Properties);

            var userCredentials = await credentialsProvider.ImpersonificateAsync(googleNetworkProperties.AdminEmail, stoppingToken);

            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = _config.ApplicationName,
            });

            var request = service.Users.List();
            request.MaxResults = 10;
            request.Customer = currentAccountCustomer;
            var response = await request.ExecuteAsync(stoppingToken);
            return response.UsersValue != null;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorContext);
            return false;
        }
    }
}