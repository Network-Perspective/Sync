using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class AuthTester(IConnectorInfoProvider connectorInfoProvider, ISlackClientFacadeFactory slackClientFacadeFactory, IVault vault, ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        var connectorInfo = connectorInfoProvider.Get();

        if (connectorInfo.GetConnectorProperties<SlackConnectorProperties>().UsesAdminPrivileges)
        {
            var isUserTokenOk = await TestUserTokenAsync(connectorInfo.Id, stoppingToken);
            var isBotTokenOk = await TestBotTokenAsync(connectorInfo.Id, stoppingToken);

            return isUserTokenOk && isBotTokenOk;
        }
        else
        {
            return await TestBotTokenAsync(connectorInfo.Id, stoppingToken);
        }
    }

    private async Task<bool> TestBotTokenAsync(Guid connectorId, CancellationToken stoppingToken)
    {
        try
        {
            var key = string.Format(SlackKeys.BotTokenKeyPattern, connectorId);
            var token = await vault.GetSecretAsync(key, stoppingToken);
            var facade = slackClientFacadeFactory.CreateUnauthorized();
            var result = await facade.TestTokenAsync(token, stoppingToken);
            return result.IsOk;

        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector's '{connectorId}' is not authorized using bot token", connectorId);
            return false;
        }
    }

    private async Task<bool> TestUserTokenAsync(Guid connectorId, CancellationToken stoppingToken)
    {
        try
        {
            var key = string.Format(SlackKeys.BotTokenKeyPattern, connectorId);
            var token = await vault.GetSecretAsync(key, stoppingToken);
            var facade = slackClientFacadeFactory.CreateUnauthorized();
            var result = await facade.TestTokenAsync(token, stoppingToken);
            return result.IsOk;

        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Connector's '{connectorId}' is not authorized using user token", connectorId);
            return false;
        }
    }
}