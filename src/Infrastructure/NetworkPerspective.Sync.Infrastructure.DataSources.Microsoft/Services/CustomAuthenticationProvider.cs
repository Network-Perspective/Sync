using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal class CustomAuthenticationProvider(IConfidentialClientAppProvider appProvider, IConnectorContextAccessor connectorContextAccessor, ICachedVault vault, IOptions<AuthConfig> authOptions, IStatusLogger statusLogger, ILogger<CustomAuthenticationProvider> logger) : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken stoppingToken = default)
    {
        try
        {
            var app = await appProvider.GetAsync(stoppingToken);
            var userKey = string.Format(MicrosoftKeys.UserKeyPattern, connectorContextAccessor.Context.ConnectorId.ToString());
            var user = await vault.GetSecretAsync(userKey, stoppingToken);
            var acc = await app.GetAccountAsync(user.ToSystemString());

            var result = await app
                .AcquireTokenSilent(authOptions.Value.Scopes, acc)
                .ExecuteAsync(stoppingToken);

            request.Headers.Add("Authorization", result.CreateAuthorizationHeader());
        }
        catch (MsalUiRequiredException ex)
        {
            logger.LogError(ex, "Unable to authenticate request");
            await statusLogger.LogErrorAsync("Request authentication failed. Please perform OAuth process", stoppingToken);

        }
    }
}