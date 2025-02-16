using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Authentication.Azure;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

using Polly;
using Polly.Extensions.Http;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal interface IMicrosoftClientFactory
{
    Task<GraphServiceClient> GetMicrosoftClientAsync(CancellationToken stoppingToken = default);
}

internal class MicrosoftClientFactory : IMicrosoftClientFactory
{
    private readonly ICachedVault _vault;
    private readonly IConnectorContextAccessor _connectorContextProvider;
    private readonly CustomAuthenticationProvider _customAuthenticationProvider;
    private readonly PolicyHttpMessageHandler _retryHandler;

    public MicrosoftClientFactory(ICachedVault vault, IConnectorContextAccessor connectorContextProvider, CustomAuthenticationProvider customAuthenticationProvider, IOptions<ResiliencyConfig> resiliencyOptions, ILoggerFactory loggerFactory)
    {
        _vault = vault;
        _connectorContextProvider = connectorContextProvider;
        _customAuthenticationProvider = customAuthenticationProvider;
        var retryLogger = loggerFactory.CreateLogger<GraphServiceClient>();

        void OnRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timespan)
        {
            retryLogger.LogInformation(result.Exception, "Problem occured on calling graph api. Next attempt in {timespan}", timespan);
        };

        var policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(resiliencyOptions.Value.Retries, OnRetry);

        _retryHandler = new PolicyHttpMessageHandler(policy);
    }

    public async Task<GraphServiceClient> GetMicrosoftClientAsync(CancellationToken stoppingToken = default)
    {
        var authProvider = await BuildAuthProvider(stoppingToken);
        var httpClient = BuildHttpClient();

        return new GraphServiceClient(httpClient, authProvider);
    }

    private async Task<IAuthenticationProvider> BuildAuthProvider(CancellationToken stoppingToken)
    {
        var connectorProperties = new MicrosoftConnectorProperties(_connectorContextProvider.Context.Properties);

        var clientIdKey = connectorProperties.SyncMsTeams == true
            ? MicrosoftKeys.MicrosoftClientTeamsIdKey
            : MicrosoftKeys.MicrosoftClientBasicIdKey;
        var clientId = await _vault.GetSecretAsync(clientIdKey, stoppingToken);

        var clientSecretKey = connectorProperties.SyncMsTeams == true
            ? MicrosoftKeys.MicrosoftClientTeamsSecretKey
            : MicrosoftKeys.MicrosoftClientBasicSecretKey;
        var clientSecret = await _vault.GetSecretAsync(clientSecretKey, stoppingToken);

        if (connectorProperties.UseUserToken)
        {
            return _customAuthenticationProvider;
        }
        else
        {
            var tenantIdKey = string.Format(MicrosoftKeys.MicrosoftTenantIdPattern, _connectorContextProvider.Context.ConnectorId);
            var tenantId = await _vault.GetSecretAsync(tenantIdKey, stoppingToken);

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId.ToSystemString(), clientId.ToSystemString(), clientSecret.ToSystemString(), options);
            return new AzureIdentityAuthenticationProvider(clientSecretCredential, []);
        }
    }

    private HttpClient BuildHttpClient()
    {
        var handlers = GraphClientFactory.CreateDefaultHandlers();
        handlers.Add(_retryHandler);
        return GraphClientFactory.Create(handlers);
    }
}