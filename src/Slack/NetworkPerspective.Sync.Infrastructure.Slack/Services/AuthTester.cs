﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class AuthTester : IAuthTester
    {
        private readonly INetworkService _networkService;
        private readonly ISlackClientFacadeFactory _slackClientFacadeFactory;
        private readonly ILogger<AuthTester> _logger;

        public AuthTester(INetworkService networkService, ISlackClientFacadeFactory slackClientFacadeFactory, ILogger<AuthTester> logger)
        {
            _networkService = networkService;
            _slackClientFacadeFactory = slackClientFacadeFactory;
            _logger = logger;
        }


        public async Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var network = await _networkService.GetAsync<SlackNetworkProperties>(networkId, stoppingToken);

            if (network.Properties.UsesAdminPrivileges)
            {
                var isUserTokenOk = await TestUserTokenAsync(networkId, stoppingToken);
                var isBotTokenOk = await TestBotTokenAsync(networkId, stoppingToken);

                return isUserTokenOk && isBotTokenOk;
            }
            else
            {
                return await TestBotTokenAsync(networkId, stoppingToken);
            }
        }

        private async Task<bool> TestBotTokenAsync(Guid networkId, CancellationToken stoppingToken)
        {
            try
            {
                var facade = await _slackClientFacadeFactory.CreateWithBotTokenAsync(networkId, stoppingToken);
                await facade.TestAsync(stoppingToken);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Network '{networkId}' is not authorized", networkId);
                return false;
            }
        }

        private async Task<bool> TestUserTokenAsync(Guid networkId, CancellationToken stoppingToken)
        {
            try
            {
                var facade = await _slackClientFacadeFactory.CreateWithUserTokenAsync(networkId, stoppingToken);
                await facade.TestAsync(stoppingToken);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Network '{networkId}' is not authorized", networkId);
                return false;
            }
        }
    }
}