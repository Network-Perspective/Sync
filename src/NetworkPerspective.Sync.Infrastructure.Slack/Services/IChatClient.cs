using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Aggregation;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Mappers;
using NetworkPerspective.Sync.Infrastructure.Slack.Models;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal interface IChatClient
    {
        Task GetInteractions(IInteractionsStorage interactionsStorage, ISlackClientFacade slackClientFacade, Network<SlackNetworkProperties> network, InteractionFactory interactionFactory, TimeRange timeRange, CancellationToken stoppingToken = default);
    }

    internal class ChatClient : IChatClient
    {
        private readonly ILogger<ChatClient> _logger;

        public ChatClient(ILogger<ChatClient> logger)
        {
            _logger = logger;
        }

        public async Task GetInteractions(IInteractionsStorage interactionsStorage, ISlackClientFacade slackClientFacade, Network<SlackNetworkProperties> network, InteractionFactory interactionFactory, TimeRange timeRange, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Fetching chats...");

            var slackChannels = await GetChannels(slackClientFacade, network, stoppingToken);
            _logger.LogDebug("There are '{count}' of channels to get interactions from", slackChannels.Count());

            if (network.Properties.AutoJoinChannels)
            {
                _logger.LogDebug("Joining channels...");

                foreach (var slackChannel in slackChannels.Where(x => !x.IsPrivate))
                    await slackClientFacade.JoinChannelAsync(slackChannel.Id, stoppingToken);

                _logger.LogDebug("Joining channels Completed");
            }

            foreach (var slackChannel in slackChannels)
            {
                try
                {
                    var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());

                    _logger.LogDebug("Getting interactions from channel...");
                    var actionsAggregator = new ActionsAggregator(slackChannel.Name);

                    var channelMembers = await slackClientFacade.GetAllSlackChannelMembers(slackChannel.Id, stoppingToken);

                    _logger.LogDebug("There are {count} users in the channel", channelMembers.Count);

                    channelMembers = channelMembers
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToHashSet();

                    _logger.LogDebug("There are {count} not null users in the channel", channelMembers.Count);

                    var slackThreadsTimeRange = new TimeRange(timeRange.Start.AddDays(-30), timeRange.End);
                    _logger.LogDebug("Getting slack theads that started in range: {timeRange}", slackThreadsTimeRange);

                    var slackThreadsMessages = await slackClientFacade.GetSlackThreads(slackChannel.Id, slackThreadsTimeRange, stoppingToken);

                    _logger.LogDebug("There are {count} threads in the channel started within {timeRange}", slackThreadsMessages.Count, slackThreadsTimeRange);

                    foreach (var slackThreadMessage in slackThreadsMessages)
                    {
                        if (GetLastUpdateOfThread(slackThreadMessage) > timeRange.Start)
                        {
                            _logger.LogDebug("Synchonizing thread...");

                            actionsAggregator.Add(TimeStampMapper.SlackTimeStampToDateTime(slackThreadMessage.TimeStamp));

                            interactions.UnionWith(interactionFactory.CreateFromThreadMessage(slackThreadMessage, slackChannel.Id, channelMembers));

                            if (HasThreadReplies(slackThreadMessage))
                            {
                                _logger.LogDebug("Getting interactions from the thread replies...");

                                var slackThreadReplies = await slackClientFacade.GetAllSlackThreadReplies(slackChannel.Id, slackThreadMessage.TimeStamp, stoppingToken);

                                foreach (var slackThreadReply in slackThreadReplies)
                                    actionsAggregator.Add(TimeStampMapper.SlackTimeStampToDateTime(slackThreadReply.TimeStamp));

                                var repliesInteractions = interactionFactory.CreateFromThreadReplies(slackThreadReplies, slackChannel.Id, slackChannel.Id + slackThreadMessage.TimeStamp, slackThreadMessage.User, timeRange);

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
                    }

                    await interactionsStorage.PushInteractionsAsync(interactions, stoppingToken);

                    _logger.LogTrace(new DefaultActionsAggregatorPrinter().Print(actionsAggregator));

                    _logger.LogDebug("Getting interactions from channel completed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Couldn't get interactions from channel. Please see inner exception");
                }
            }
        }

        private async Task<IEnumerable<Channel>> GetChannels(ISlackClientFacade slackClientFacade, Network<SlackNetworkProperties> network, CancellationToken stoppingToken = default)
        {
            if (network.Properties.AutoJoinChannels)
            {
                _logger.LogDebug("Getting list of all channels for network '{networkId}'...", network.NetworkId);
                return (await slackClientFacade.GetAllSlackChannels(stoppingToken)).Select(x => new Channel(x.Id, x.Name, x.IsPrivate));
            }
            else
            {
                _logger.LogDebug("Getting list of channels, the application has been added to, for network '{networkId}'...", network.NetworkId);
                return (await slackClientFacade.GetCurrentUserChannels(stoppingToken)).Select(x => new Channel(x.Id, x.Name, x.IsPrivate));
            }
        }

        private DateTime GetLastUpdateOfThread(ConversationHistoryResponse.SingleMessage slackThreadMessage)
        {
            if (HasThreadReplies(slackThreadMessage))
                return TimeStampMapper.SlackTimeStampToDateTime(slackThreadMessage.LatestReplyTimeStamp);
            else
                return TimeStampMapper.SlackTimeStampToDateTime(slackThreadMessage.TimeStamp);
        }

        private bool HasThreadReplies(ConversationHistoryResponse.SingleMessage slackThreadMessage)
            => slackThreadMessage.ReplyCount != 0;
    }
}