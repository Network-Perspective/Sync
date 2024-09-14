using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class AuthTester : IAuthTester
{
    private readonly IAuthTesterContextAccessor _contextAccessor;
    private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
    private readonly ILogger<AuthTester> _logger;

    public AuthTester(IAuthTesterContextAccessor contextAccessor, ISlackClientFacadeFactory slackClientFacadeFactory, ILogger<AuthTester> logger)
    {
        _contextAccessor = contextAccessor;
        _slackClientFacadeFactory = slackClientFacadeFactory;
        _logger = logger;
    }


    public async Task<bool> IsAuthorizedAsync(IDictionary<string, string> networkProperties, CancellationToken stoppingToken = default)
    {
        var connectorProperties = ConnectorProperties.Create<SlackConnectorProperties>(networkProperties);

        if (connectorProperties.UsesAdminPrivileges)
        {
            var isUserTokenOk = await TestUserTokenAsync(_contextAccessor.Context.ConnectorId, stoppingToken);
            var isBotTokenOk = await TestBotTokenAsync(_contextAccessor.Context.ConnectorId, stoppingToken);

            return isUserTokenOk && isBotTokenOk;
        }
        else
        {
            return await TestBotTokenAsync(_contextAccessor.Context.ConnectorId, stoppingToken);
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