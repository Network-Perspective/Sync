using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services;

internal class PrivilegedChatJoiner(ISlackClientBotScopeFacade slackClientBotScope, ISlackClientUserScopeFacade slackClientUserScope, ILogger<PrivilegedChatJoiner> logger) : IChatJoiner
{
    public async Task JoinAsync(CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Joining public and private channels...");

        var testResponse = await slackClientBotScope.TestAsync(stoppingToken);
        var teams = await slackClientBotScope.GetTeamsListAsync(stoppingToken);

        foreach (var team in teams)
            await JoinInTeamAsync(testResponse.UserId, team.Id, stoppingToken);

        logger.LogDebug("Joining public and private channels completed");
    }

    private async Task JoinInTeamAsync(string botId, string teamId, CancellationToken stoppingToken)
    {
        var botVisibleChannels = await slackClientBotScope.GetSlackChannelsAsync(teamId, stoppingToken);
        var adminVisibleChannels = await slackClientUserScope.GetPrivateSlackChannelsAsync(stoppingToken);

        await JoinPublicChannelsAsync(botVisibleChannels, stoppingToken);
        await JoinPrivateChannelsAsync(botId, botVisibleChannels, adminVisibleChannels, stoppingToken);
    }

    private async Task JoinPublicChannelsAsync(IReadOnlyCollection<ConversationsListResponse.SingleConversation> botChannels, CancellationToken stoppingToken)
    {
        var successfulJoins = 0;
        var failedJoins = 0;
        foreach (var channel in botChannels.Where(x => !x.IsPrivate))
        {
            try
            {
                await slackClientBotScope.JoinChannelAsync(channel.Id, stoppingToken);
                successfulJoins++;
            }
            catch (Exception)
            {
                failedJoins++;
            }
        }
        logger.LogInformation("Successfully joined {Count} public channels", successfulJoins);
        if (failedJoins > 0) logger.LogInformation("Failed to join {Count} public channels", failedJoins);
    }

    private async Task JoinPrivateChannelsAsync(string botId, IReadOnlyCollection<ConversationsListResponse.SingleConversation> botChannels, IReadOnlyCollection<AdminConversationsListResponse.SingleConversation> adminChannels, CancellationToken stoppingToken)
    {
        var successfulJoins = 0;
        var failedJoins = 0;
        foreach (var channel in adminChannels)
        {
            if (IsAlreadyMemberOfChannel(botChannels, channel)) continue;

            try
            {
                await slackClientUserScope.JoinChannelAsync(channel.Id, botId, stoppingToken);
                successfulJoins++;
            }
            catch (Exception)
            {
                failedJoins++;
            }
        }
        logger.LogInformation("Successfully joined {Count} private channels", successfulJoins);
        if (failedJoins > 0) logger.LogInformation("Failed to join {Count} private channels", failedJoins);
    }

    private static bool IsAlreadyMemberOfChannel(IReadOnlyCollection<ConversationsListResponse.SingleConversation> botChannels, AdminConversationsListResponse.SingleConversation channel)
        => botChannels.Any(x => x.Id == channel.Id);
}