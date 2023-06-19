using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal interface IChatClient
    {
        Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, ISlackClientBotScopeFacade slackClientFacade, Network<SlackNetworkProperties> network, InteractionFactory interactionFactory, TimeRange timeRange, CancellationToken stoppingToken = default);
    }

    internal class ChatClient : IChatClient
    {
        private const string TaskCaption = "Synchronizing chat interactions";
        private const string TaskDescription = "Fetching chats metadata from Slack API";

        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<ChatClient> _logger;

        public ChatClient(ITasksStatusesCache tasksStatusesCache, ILogger<ChatClient> logger)
        {
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, ISlackClientBotScopeFacade slackClientFacade, Network<SlackNetworkProperties> network, InteractionFactory interactionFactory, TimeRange timeRange, CancellationToken stoppingToken = default)
        {
            var teams = await slackClientFacade.GetTeamsListAsync(stoppingToken);

            var result = SyncResult.Empty;

            foreach (var team in teams)
            {
                var channelsIds = (await slackClientFacade.GetCurrentUserChannelsAsync(team.Id, stoppingToken))
                    .Select(x => x.Id);

                async Task ReportProgressCallbackAsync(double progressRate)
                {
                    var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                    await _tasksStatusesCache.SetStatusAsync(network.NetworkId, taskStatus, stoppingToken);
                }

                Task<SingleTaskResult> SingleTaskAsync(string channelId)
                    => SyncSingleChannnelInteractionsAsync(stream, slackClientFacade, interactionFactory, timeRange, channelId, stoppingToken);

                _logger.LogInformation("Evaluating interactions based on chat for timerange {timerange} for {count} channels in team '{team}'...", timeRange, channelsIds.Count(), team.Name);

                var partialResult = await ParallelSyncTask.RunAsync(channelsIds, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
                result = SyncResult.Combine(result, partialResult);

                _logger.LogInformation("Evaluation of interactions based on chat for timerange '{timerange}' for team '{team}' completed", timeRange, team.Name);
            }

            return result;
        }

        private async Task<SingleTaskResult> SyncSingleChannnelInteractionsAsync(IInteractionsStream stream, ISlackClientBotScopeFacade slackClientFacade, InteractionFactory interactionFactory, TimeRange timeRange, string slackChannelId, CancellationToken stoppingToken)
        {
            _logger.LogDebug("Getting interactions from channel...");
            var interactionsCount = 0;

            var channelMembers = await slackClientFacade.GetAllSlackChannelMembersAsync(slackChannelId, stoppingToken);

            _logger.LogDebug("There are {count} users in the channel", channelMembers.Count);

            channelMembers = channelMembers
                .Where(x => !string.IsNullOrEmpty(x))
                .ToHashSet();

            _logger.LogDebug("There are {count} not null users in the channel", channelMembers.Count);

            var slackThreadsTimeRange = new TimeRange(timeRange.Start.AddDays(-30), timeRange.End);
            _logger.LogDebug("Getting slack theads that started in range: {timeRange}", slackThreadsTimeRange);

            var slackThreadsMessages = await slackClientFacade.GetSlackThreadsAsync(slackChannelId, slackThreadsTimeRange, stoppingToken);

            _logger.LogDebug("There are {count} threads in the channel started within {timeRange}", slackThreadsMessages.Count, slackThreadsTimeRange);

            foreach (var slackThreadMessage in slackThreadsMessages)
            {
                var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());

                if (GetLastUpdateOfThread(slackThreadMessage) > timeRange.Start)
                {
                    _logger.LogDebug("Synchonizing thread...");

                    interactions.UnionWith(interactionFactory.CreateFromThreadMessage(slackThreadMessage, slackChannelId, channelMembers));

                    if (HasThreadReplies(slackThreadMessage))
                    {
                        _logger.LogDebug("Getting interactions from the thread replies...");

                        var slackThreadReplies = await slackClientFacade.GetAllSlackThreadRepliesAsync(slackChannelId, slackThreadMessage.TimeStamp, stoppingToken);

                        var repliesInteractions = interactionFactory.CreateFromThreadReplies(slackThreadReplies, slackChannelId, slackChannelId + slackThreadMessage.TimeStamp, slackThreadMessage.User, timeRange);

                        if (slackThreadReplies.Any())
                        {
                            var threadStart = TimeStampMapper.SlackTimeStampToDateTime(slackThreadMessage.TimeStamp);
                            var threadEnd = TimeStampMapper.SlackTimeStampToDateTime(slackThreadReplies.Last().TimeStamp);
                            var threadLifetime = threadEnd - threadStart;
                            _logger.LogInformation("Thread lifetime: {lifetime}", threadLifetime);

                            if (threadLifetime.TotalDays > 30)
                                _logger.LogWarning("Thread lifetime longer than 30 days: {lifetime}", threadLifetime);
                        }

                        _logger.LogDebug("There are {count} interactions from the thread replies", repliesInteractions.Count);

                        interactions.UnionWith(repliesInteractions);

                        _logger.LogDebug("Getting interactions from the thread replies completed");
                    }
                    else
                    {
                        _logger.LogDebug("The thread has no replies");
                    }

                    _logger.LogDebug("Thread synchronization completed");
                }
                var sentInteractionsCount = await stream.SendAsync(interactions);
                interactionsCount += sentInteractionsCount;
            }

            _logger.LogDebug("Getting interactions from channel completed");
            return new SingleTaskResult(interactionsCount);
        }

        private DateTime GetLastUpdateOfThread(ConversationHistoryResponse.SingleMessage slackThreadMessage)
        {
            if (HasThreadReplies(slackThreadMessage))
                return TimeStampMapper.SlackTimeStampToDateTime(slackThreadMessage.LatestReplyTimeStamp);
            else
                return TimeStampMapper.SlackTimeStampToDateTime(slackThreadMessage.TimeStamp);
        }

        private static bool HasThreadReplies(ConversationHistoryResponse.SingleMessage slackThreadMessage)
            => slackThreadMessage.ReplyCount != 0;
    }
}