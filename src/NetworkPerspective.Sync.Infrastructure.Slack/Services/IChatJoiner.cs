using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Slack.Client;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal interface IChatJoiner
    {
        Task JoinAsync(CancellationToken stoppingToken = default);
    }

    internal class UnprivilegedChatJoiner : IChatJoiner
    {
        private readonly ISlackClientBotScopeFacade _slackClientBotScope;
        private readonly ILogger<UnprivilegedChatJoiner> _logger;

        public UnprivilegedChatJoiner(ISlackClientBotScopeFacade slackClientBotScope, ILogger<UnprivilegedChatJoiner> logger)
        {
            _slackClientBotScope = slackClientBotScope;
            _logger = logger;
        }

        public async Task JoinAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Joining public channels...");

            var teams = await _slackClientBotScope.GetTeamsListAsync(stoppingToken);

            foreach (var team in teams)
            {
                var channels = await _slackClientBotScope.GetSlackChannelsAsync(team.Id, stoppingToken);

                foreach (var channel in channels.Where(x => !x.IsPrivate))
                    await _slackClientBotScope.JoinChannelAsync(channel.Id, stoppingToken);
            }

            _logger.LogDebug("Joining public channels completed");
        }
    }

    internal class PrivilegedChatJoiner : IChatJoiner
    {
        private readonly ISlackClientBotScopeFacade _slackClientBotScope;
        private readonly ISlackClientUserScopeFacade _slackClientUserScope;
        private readonly ILogger<PrivilegedChatJoiner> _logger;

        public PrivilegedChatJoiner(ISlackClientBotScopeFacade slackClientBotScope, ISlackClientUserScopeFacade slackClientUserScope, ILogger<PrivilegedChatJoiner> logger)
        {
            _slackClientBotScope = slackClientBotScope;
            _slackClientUserScope = slackClientUserScope;
            _logger = logger;
        }

        public async Task JoinAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Joining public and private channels...");

            var testResponse = await _slackClientBotScope.TestAsync(stoppingToken);
            var teams = await _slackClientBotScope.GetTeamsListAsync(stoppingToken);

            foreach (var team in teams)
            {
                var botChannels = await _slackClientBotScope.GetSlackChannelsAsync(team.Id, stoppingToken);

                foreach (var channel in botChannels.Where(x => !x.IsPrivate))
                    await _slackClientBotScope.JoinChannelAsync(channel.Id, stoppingToken);

                var adminChannels = await _slackClientUserScope.GetPrivateSlackChannelsAsync(stoppingToken);

                foreach (var channel in adminChannels)
                {
                    if(!botChannels.Select(x => x.Id).Contains(channel.Id))
                        await _slackClientUserScope.JoinChannelAsync(channel.Id, testResponse.UserId, stoppingToken);
                }
            }

            _logger.LogDebug("Joining public and private channels completed");
        }
    }
}
