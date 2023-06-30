using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
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
                await JoinInTeamAsync(testResponse.UserId, team.Id, stoppingToken);

            _logger.LogDebug("Joining public and private channels completed");
        }

        private async Task JoinInTeamAsync(string botId, string teamId, CancellationToken stoppingToken)
        {
            var botVisibleChannels = await _slackClientBotScope.GetSlackChannelsAsync(teamId, stoppingToken);
            var adminVisibleChannels = await _slackClientUserScope.GetPrivateSlackChannelsAsync(stoppingToken);

            await JoinPublicChannelsAsync(botVisibleChannels, stoppingToken);
            await JoinPrivateChannelsAsync(botId, botVisibleChannels, adminVisibleChannels, stoppingToken);
        }

        private async Task JoinPublicChannelsAsync(IReadOnlyCollection<ConversationsListResponse.SingleConversation> botChannels, CancellationToken stoppingToken)
        {
            foreach (var channel in botChannels.Where(x => !x.IsPrivate))
                await _slackClientBotScope.JoinChannelAsync(channel.Id, stoppingToken);
        }

        private async Task JoinPrivateChannelsAsync(string botId, IReadOnlyCollection<ConversationsListResponse.SingleConversation> botChannels, IReadOnlyCollection<AdminConversationsListResponse.SingleConversation> adminChannels, CancellationToken stoppingToken)
        {
            foreach (var channel in adminChannels)
            {
                if (!IsAlreadyMemberOfChannel(botChannels, channel))
                    await _slackClientUserScope.JoinChannelAsync(channel.Id, botId, stoppingToken);
            }
        }

        private static bool IsAlreadyMemberOfChannel(IReadOnlyCollection<ConversationsListResponse.SingleConversation> botChannels, AdminConversationsListResponse.SingleConversation channel)
            => botChannels.Any(x => x.Id == channel.Id);
    }
}