using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Teams.Item.Channels.Item.Messages.Delta;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;

using GraphChannel = Microsoft.Graph.Models.Channel;
using ChatMessage = Microsoft.Graph.Models.ChatMessage;
using InternalChannel = NetworkPerspective.Sync.Infrastructure.Microsoft.Models.Channel;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IChannelsClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IEnumerable<InternalChannel> channels, IInteractionsStream stream, IChannelsInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
        Task<List<InternalChannel>> GetAllChannelsAsync(CancellationToken stoppingToken = default);
    }

    internal class ChannelsClient : IChannelsClient
    {
        private const string TaskCaption = "Synchronizing channels interactions";
        private const string TaskDescription = "Fetching channels metadata from Microsoft API";

        private readonly GraphServiceClient _graphClient;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ChannelsClient> _logger;

        public ChannelsClient(GraphServiceClient graphClient, ITasksStatusesCache tasksStatusesCache, ILoggerFactory loggerFactory)
        {
            _graphClient = graphClient;
            _tasksStatusesCache = tasksStatusesCache;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ChannelsClient>();
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IEnumerable<InternalChannel> channels, IInteractionsStream stream, IChannelsInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(InternalChannel channel)
                => GetSingleChannelInteractionsAsync(context, stream, channel, interactionFactory, context.TimeRange, stoppingToken);

            var filteredChannels = new ChannelFilter(_loggerFactory.CreateLogger<ChannelFilter>())
                .Filter(channels, context.NetworkConfig.EmailFilter);

            _logger.LogInformation("Evaluating interactions based on {count} channels for '{timerange}'...", filteredChannels.Count, context.TimeRange);
            var result = await ParallelSyncTask<InternalChannel>.RunAsync(filteredChannels, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on {count} channels for '{timerange}' completed", filteredChannels.Count, context.TimeRange);

            return result;
        }

        public async Task<List<InternalChannel>> GetAllChannelsAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Getting all Channels in all Teams...");

            var result = new List<InternalChannel>();

            var teamsIds = await GetAllTeamsIdsAsync(stoppingToken);

            foreach (var teamId in teamsIds)
            {
                var channels = await GetAllChannelsInSingleTeamAsync(teamId, stoppingToken);

                result.AddRange(channels);
            }

            _logger.LogDebug("Got {count} Channels", result.Count);

            return result;
        }

        private async Task<List<string>> GetAllTeamsIdsAsync(CancellationToken stoppingToken)
        {
            var result = new List<string>();
            var teamsResponse = await _graphClient
                .Teams
                .GetAsync(x =>
                {
                    x.QueryParameters = new()
                    {
                        Select = new[]
                        {
                            nameof(Team.Id)
                        }
                    };
                }, stoppingToken);

            var pageIterator = PageIterator<Team, TeamCollectionResponse>
                .CreatePageIterator(_graphClient, teamsResponse,
                team =>
                {
                    result.Add(team.Id);
                    return true;
                },
                request =>
                {
                    return request;
                });
            await pageIterator.IterateAsync(stoppingToken);

            return result;
        }

        private async Task<List<InternalChannel>> GetAllChannelsInSingleTeamAsync(string teamId, CancellationToken stoppingToken)
        {
            var result = new List<InternalChannel>();
            var channelsResponse = await _graphClient
                .Teams[teamId].AllChannels
                .GetAsync(x =>
                {
                    x.QueryParameters = new()
                    {
                        Select = new[]
                        {
                            nameof(GraphChannel.Id),
                            nameof(GraphChannel.DisplayName)
                        }
                    };
                }, stoppingToken);

            var pageIterator = PageIterator<GraphChannel, ChannelCollectionResponse>
                .CreatePageIterator(_graphClient, channelsResponse,
                async channel =>
                {
                    var identifier = ChannelIdentifier.Create(teamId, channel.Id);
                    var userIds = await GetChannelMembersIdsAsync(identifier, stoppingToken);
                    var internalChannel = new InternalChannel(identifier, channel.DisplayName, userIds);

                    result.Add(internalChannel);
                    return true;
                },
                request =>
                {
                    return request;
                });
            await pageIterator.IterateAsync(stoppingToken);

            return result;
        }

        private async Task<SingleTaskResult> GetSingleChannelInteractionsAsync(SyncContext context, IInteractionsStream stream, InternalChannel channel, IChannelsInteractionFactory interactionFactory, Application.Domain.TimeRange timeRange, CancellationToken stoppingToken)
        {
            _logger.LogDebug("Getting interactions from channel...");

            var interactionsCount = 0;

            var threads = await GetThreadsAsync(channel.Id, context.TimeRange.Start, stoppingToken);

            foreach (var thread in threads)
            {
                var threadInteractions = interactionFactory.CreateFromThreadMessage(thread, channel);
                var sentThreadInteractionsCount = await stream.SendAsync(threadInteractions);
                interactionsCount += sentThreadInteractionsCount;

                var replies = await GetThreadsRepliesAsync(channel.Id, thread, stoppingToken);
                var repliesInteractions = interactionFactory.CreateFromThreadRepliesMessage(replies, channel.Id.ChannelId, thread.Id, thread.From.User.Id, timeRange);
                var sentRepliesInteractionsCount = await stream.SendAsync(repliesInteractions);
                interactionsCount += sentRepliesInteractionsCount;
            }

            _logger.LogDebug("Getting interactions from channel completed");


            return new SingleTaskResult(interactionsCount);
        }

        private async Task<ISet<string>> GetChannelMembersIdsAsync(ChannelIdentifier channel, CancellationToken stoppingToken)
        {
            var result = new HashSet<string>();

            var membersResponse = await _graphClient
                .Teams[channel.TeamId]
                .Channels[channel.ChannelId]
                .Members
                .GetAsync(cancellationToken: stoppingToken);

            var pageIterator = PageIterator<ConversationMember, ConversationMemberCollectionResponse>
                .CreatePageIterator(_graphClient, membersResponse,
                member =>
                {
                    if (member is AadUserConversationMember aadMember)
                        result.Add(aadMember.Email);
                    else
                        result.Add(member.Id);

                    return true;
                },
                request =>
                {
                    return request;
                });
            await pageIterator.IterateAsync(stoppingToken);

            return result;
        }
        private async Task<List<ChatMessage>> GetThreadsAsync(ChannelIdentifier channel, DateTime from, CancellationToken stoppingToken)
        {
            var filterString = $"lastModifiedDateTime gt {from:s}Z";
            var result = new List<ChatMessage>();

            var threadsResponse = await _graphClient
                .Teams[channel.TeamId]
                .Channels[channel.ChannelId]
                .Messages
                .Delta
                .GetAsDeltaGetResponseAsync(x =>
                {
                    x.QueryParameters = new()
                    {
                        Filter = filterString
                    };
                }, stoppingToken);


            var pageIterator = PageIterator<ChatMessage, DeltaGetResponse>
                .CreatePageIterator(_graphClient, threadsResponse,
                thread =>
                {
                    result.Add(thread);
                    return true;
                },
                request =>
                {
                    return request;
                });
            await pageIterator.IterateAsync(stoppingToken);

            return result
                .Where(x => x.MessageType == ChatMessageType.Message)
                .ToList();
        }

        private async Task<List<ChatMessage>> GetThreadsRepliesAsync(ChannelIdentifier channel, ChatMessage thread, CancellationToken stoppingToken)
        {
            var result = new List<ChatMessage>();

            var threadsRepliesResponse = await _graphClient
                .Teams[channel.TeamId]
                .Channels[channel.ChannelId]
                .Messages[thread.Id]
                .Replies
                .GetAsync(x =>
                {

                }, stoppingToken);

            var pageIterator = PageIterator<ChatMessage, ChatMessageCollectionResponse>
                .CreatePageIterator(_graphClient, threadsRepliesResponse,
                reply =>
                {
                    result.Add(reply);
                    return true;
                },
                request =>
                {
                    return request;
                });
            await pageIterator.IterateAsync(stoppingToken);

            return result
                .Where(x => x.MessageType == ChatMessageType.Message)
                .ToList();
        }
    }
}