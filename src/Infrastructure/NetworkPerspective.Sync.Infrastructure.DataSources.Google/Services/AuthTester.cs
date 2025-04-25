using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.Credentials;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal class AuthTester(IOptions<GoogleConfig> config, ICredentialsService credentialsService, IConnectorContextAccessor connectorContextProvider, ILogger<AuthTester> logger) : IAuthTester
{
    private readonly GoogleConfig _config = config.Value;

    public async Task<AuthStatus> GetStatusAsync(CancellationToken stoppingToken = default)
    {
        const string currentAccountCustomer = "my_customer";

        var connectorContext = connectorContextProvider.Context;
        try
        {
            logger.LogInformation("Checking if connector '{connectorId}' is authorized", connectorContext.ConnectorId);
            var googleNetworkProperties = new GoogleConnectorProperties(connectorContext.Properties);

            var userCredentials = googleNetworkProperties.UseUserToken
                ? await credentialsService.GetUserCredentialsAsync(stoppingToken)
                : await credentialsService.ImpersonificateAsync(googleNetworkProperties.AdminEmail, stoppingToken);


            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = _config.ApplicationName,
            });

            var request = service.Users.List();
            request.MaxResults = 10;
            request.Customer = currentAccountCustomer;
            var response = await request.ExecuteAsync(stoppingToken);
            var isAuthorized = response.UsersValue != null;
            var props = await GetAuthPropsAsync(googleNetworkProperties.UseUserToken, stoppingToken);
            return AuthStatus.WithProperties(isAuthorized, props);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorContext.ConnectorId);
            return AuthStatus.Create(false);
        }
    }

    private async Task<Dictionary<string, string>> GetAuthPropsAsync(bool useUserToken, CancellationToken stoppingToken)
    {
        if (useUserToken)
            return [];

        try
        {
            var clientId = await credentialsService.GetServiceAccountClientIdAsync(stoppingToken);

            return new Dictionary<string, string>
            {
                { "client-id", clientId },
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to '{key}'", GoogleKeys.TokenKey);
            return [];
        }
    }
}