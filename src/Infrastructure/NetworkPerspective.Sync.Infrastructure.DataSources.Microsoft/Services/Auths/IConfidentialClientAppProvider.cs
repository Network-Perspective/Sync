using System.Threading;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;

internal interface IConfidentialClientAppProvider
{
    Task<IConfidentialClientApplication> GetAsync(CancellationToken stoppingToken = default);
    Task<IConfidentialClientApplication> GetWithRedirectUrlAsync(string redirectUri, CancellationToken stoppingToken = default);
}

internal class ConfidentialClientAppProvider(IVault vault, IUserTokenCacheVaultBinder cacheVaultBinder) : IConfidentialClientAppProvider
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private ConfidentialClientApplicationBuilder _builder = null;

    public async Task<IConfidentialClientApplication> GetAsync(CancellationToken stoppingToken = default)
    {
        var builder = await GetBuilderAsync(stoppingToken);
        var app = builder.Build();

        cacheVaultBinder.Bind(app.UserTokenCache);

        return app;
    }

    public async Task<IConfidentialClientApplication> GetWithRedirectUrlAsync(string redirectUri, CancellationToken stoppingToken = default)
    {
        var builder = await GetBuilderAsync(stoppingToken);
        var app = builder
            .WithRedirectUri(redirectUri)
            .Build();

        cacheVaultBinder.Bind(app.UserTokenCache);

        return app;
    }

    private async Task<ConfidentialClientApplicationBuilder> GetBuilderAsync(CancellationToken stoppingToken)
    {
        await _semaphore.WaitAsync(stoppingToken);

        try
        {
            if (_builder is null)
                _builder = await CreateBuilderAsync(stoppingToken);

            return _builder;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<ConfidentialClientApplicationBuilder> CreateBuilderAsync(CancellationToken stoppingToken)
    {
        var clientId = await vault.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(MicrosoftKeys.MicrosoftClientBasicSecretKey, stoppingToken);

        return ConfidentialClientApplicationBuilder
            .Create(clientId.ToSystemString())
            .WithClientSecret(clientSecret.ToSystemString());
    }
}