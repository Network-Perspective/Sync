using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

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
        var attemptTimeout = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(60),                    
            TimeoutStrategy.Pessimistic);               
        
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>() 
            .Or<TimeoutException>()
            .Or<TaskCanceledException>(ex => ex.InnerException is TimeoutException)
            .Or<OperationCanceledException>(ex => ex.InnerException is TimeoutException)
            .WaitAndRetryAsync(resiliencyOptions.Value.Retries, OnRetry);

        var resiliency = Policy.WrapAsync(retry, attemptTimeout);

        _retryHandler = new PolicyHttpMessageHandler(resiliency);
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
        var handlers = KiotaClientFactory.CreateDefaultHandlers();
        
        // remove kiota retry handler
        handlers = handlers.Where(h=>h is not RetryHandler).ToList();
        // insert polly
        handlers.Insert(0, _retryHandler);
        handlers.Add(new GraphTelemetryHandler()); 
        
        var httpClient = GraphClientFactory.Create(handlers);
        
        // Increase total timeout 
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        return httpClient;
    }
}