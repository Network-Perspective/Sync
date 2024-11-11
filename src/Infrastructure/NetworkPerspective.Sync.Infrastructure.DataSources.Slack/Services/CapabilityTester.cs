using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class CapabilityTester(ConnectorType connectorType, IVault vault, ILogger<CapabilityTester> logger) : ICapabilityTester
{
    public async Task<IEnumerable<ConnectorType>> GetCapabilitiesAsync(CancellationToken stoppingToken = default)
    {
        var areClientSecretsSet = await vault.CanGetSecretsAsync(stoppingToken, SlackKeys.SlackClientIdKey, SlackKeys.SlackClientIdKey);
        logger.LogDebug("Client Id and Secret are accessible");

        if (areClientSecretsSet)
            return [connectorType];
        else
            return [];
    }
}