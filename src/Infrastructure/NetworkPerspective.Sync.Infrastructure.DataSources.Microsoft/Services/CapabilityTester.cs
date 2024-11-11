using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal class CapabilityTester(ConnectorType connectorType, IVault vault, ILogger<CapabilityTester> logger) : ICapabilityTester
{
    public async Task<IEnumerable<ConnectorType>> GetCapabilitiesAsync(CancellationToken stoppingToken = default)
    {
        var canSyncUsingBasicClient = await vault.CanGetSecretsAsync(stoppingToken, MicrosoftKeys.MicrosoftClientBasicIdKey, MicrosoftKeys.MicrosoftClientBasicSecretKey);
        logger.LogDebug("Basic client secrets are accessible");

        var canSyncUsingTeamsClient = await vault.CanGetSecretsAsync(stoppingToken, MicrosoftKeys.MicrosoftClientTeamsIdKey, MicrosoftKeys.MicrosoftClientTeamsSecretKey);
        logger.LogDebug("Teams client secrets are accessible");

        if (canSyncUsingBasicClient || canSyncUsingTeamsClient)
            return [connectorType];
        else
            return [];
    }
}