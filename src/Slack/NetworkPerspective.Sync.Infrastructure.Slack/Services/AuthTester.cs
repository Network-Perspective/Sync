using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class AuthTester : IAuthTester
    {
        private readonly IConnectorService _connectorService;
        private readonly IConnectorInfoProvider _connectorInfoProvider;
        private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
        private readonly ILogger<AuthTester> _logger;

        public AuthTester(IConnectorService connectorService, IConnectorInfoProvider connectorInfoProvider, ISlackClientFacadeFactory slackClientFacadeFactory, ILogger<AuthTester> logger)
        {
            _connectorService = connectorService;
            _connectorInfoProvider = connectorInfoProvider;
            _slackClientFacadeFactory = slackClientFacadeFactory;
            _logger = logger;
        }


        public async Task<bool> IsAuthorizedAsync(CancellationToken stoppingToken = default)
        {
            var connectorInfo = _connectorInfoProvider.Get();
            var connector = await _connectorService.GetAsync<SlackNetworkProperties>(connectorInfo.Id, stoppingToken);

            if (connector.Properties.UsesAdminPrivileges)
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
                _logger.LogInformation(ex, "Connector '{connector}' is not authorized", connectorId);
                return false;
            }
        }

        private async Task<bool> TestUserTokenAsync(Guid networkId, CancellationToken stoppingToken)
        {
            try
            {
                var facade = _slackClientFacadeFactory.CreateWithUserToken(stoppingToken);
                await facade.TestAsync(stoppingToken);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Connector '{connectorId}' is not authorized", networkId);
                return false;
            }
        }
    }
}