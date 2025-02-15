using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;

internal interface IUserTokenCacheVaultBinder
{
    void Bind(ITokenCache tokenCache);
}

internal class UserTokenCacheVaultBinder(ICachedVault vault, IConnectorContextAccessor connectorContextAccessor, ILogger<UserTokenCacheVaultBinder> logger) : IUserTokenCacheVaultBinder
{
    public void Bind(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccessAsync(BeforeAccessNotification);
        tokenCache.SetAfterAccessAsync(AfterAccessNotification);
    }

    private async Task BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        if (args.HasStateChanged)
            return;

        var key = string.Format(MicrosoftKeys.UserTokenCacheKeyPattern, connectorContextAccessor.Context.ConnectorId);
        var content = await vault.GetSecretAsync(key, args.CancellationToken);
        var data = Convert.FromBase64String(content.ToSystemString());
        args.TokenCache.DeserializeMsalV3(data);
    }

    private async Task AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        if (args.HasStateChanged)
        {
            var key = string.Format(MicrosoftKeys.UserTokenCacheKeyPattern, connectorContextAccessor.Context.ConnectorId.ToString());
            await vault.ClearCacheAsync(key, args.CancellationToken);
            var cacheData = args.TokenCache.SerializeMsalV3();
            var cacheDataString = Convert.ToBase64String(cacheData);

            await vault.SetSecretAsync(key, cacheDataString.ToSecureString(), args.CancellationToken);
        }
    }
}