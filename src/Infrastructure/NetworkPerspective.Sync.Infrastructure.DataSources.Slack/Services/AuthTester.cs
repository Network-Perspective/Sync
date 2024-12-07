﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class AuthTester : IAuthTester
{
    private readonly IConnectorInfoProvider _connectorInfoProvider;
    private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
    private readonly ILogger<AuthTester> _logger;

    public AuthTester(IConnectorInfoProvider connectorInfoProvider, ISlackClientFacadeFactory slackClientFacadeFactory, ILogger<AuthTester> logger)
    {
        _connectorInfoProvider = connectorInfoProvider;
        _slackClientFacadeFactory = slackClientFacadeFactory;
        _logger = logger;
    }


    public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
    {
        var connectorInfo = _connectorInfoProvider.Get();

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
            var facade = _slackClientFacadeFactory.CreateWithBotToken(stoppingToken);
            await facade.TestAsync(stoppingToken);
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorId);
            return false;
        }
    }

    private async Task<bool> TestUserTokenAsync(Guid connectorId, CancellationToken stoppingToken)
    {
        try
        {
            var facade = _slackClientFacadeFactory.CreateWithUserToken(stoppingToken);
            await facade.TestAsync(stoppingToken);
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", connectorId);
            return false;
        }
    }
}