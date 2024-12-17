﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class AuthTester(IConnectorContextAccessor connectorContextProvider, ISlackClientFacadeFactory slackClientFacadeFactory, IVault vault, ILogger<AuthTester> logger) : IAuthTester
{
    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        var connectorContext = connectorContextProvider.Context;

        if (connectorContext.GetConnectorProperties<SlackConnectorProperties>().UsesAdminPrivileges)
        {
            var isUserTokenOk = await TestUserTokenAsync(connectorContext.ConnectorId, stoppingToken);
            var isBotTokenOk = await TestBotTokenAsync(connectorContext.ConnectorId, stoppingToken);

            return isUserTokenOk && isBotTokenOk;
        }
        else
        {
            return await TestBotTokenAsync(connectorContext.ConnectorId, stoppingToken);
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