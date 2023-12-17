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

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IChannelsClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, CancellationToken stoppingToken = default);
    }

    internal class ChannelsClient : IChannelsClient
    {
        private const string TaskCaption = "Synchronizing channels interactions";
        private const string TaskDescription = "Fetching channels metadata from Microsoft API";

        private readonly GraphServiceClient _graphClient;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly ILogger<ChannelsClient> _logger;

        public ChannelsClient(GraphServiceClient graphClient, ITasksStatusesCache tasksStatusesCache, ILogger<ChannelsClient> logger)
        {
            _graphClient = graphClient;
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = new SingleTaskStatus(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.NetworkId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(ChannelIdentifier channel)
                => GetSingleChannelInteractionsAsync(context, stream, channel, stoppingToken);

            var channels = await GetAllChannelsIdentifiersAsync(stoppingToken);

            _logger.LogInformation("Evaluating interactions based on chat for '{timerange}' for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<ChannelIdentifier>.RunAsync(channels, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on chat for '{timerange}' completed", context.TimeRange);

            return result;
        }

        private async Task<List<ChannelIdentifier>> GetAllChannelsIdentifiersAsync(CancellationToken stoppingToken)
        {
            var result = new List<ChannelIdentifier>();

            var teamsIds = await GetAllTeamsIdsAsync(stoppingToken);

            foreach (var teamId in teamsIds)
            {
                var channelsIds = await GetAllChannelsIdsInSingleTeamAsync(teamId, stoppingToken);
                result.AddRange(channelsIds.Select(x => new ChannelIdentifier { ChannelId = x, TeamId = teamId }));
            }

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

        private async Task<List<string>> GetAllChannelsIdsInSingleTeamAsync(string teamId, CancellationToken stoppingToken)
        {
            var result = new List<string>();
            var channelsResponse = await _graphClient
                .Teams[teamId].AllChannels
                .GetAsync(x =>
                {
                    x.QueryParameters = new()
                    {
                        Select = new[]
                        {
                            nameof(Channel.Id)
                        }
                    };
                }, stoppingToken);

            var pageIterator = PageIterator<Channel, ChannelCollectionResponse>
                .CreatePageIterator(_graphClient, channelsResponse,
                channel =>
                {
                    result.Add(channel.Id);
                    return true;
                },
                request =>
                {
                    return request;
                });
            await pageIterator.IterateAsync(stoppingToken);

            return result;
        }

        private async Task<SingleTaskResult> GetSingleChannelInteractionsAsync(SyncContext context, IInteractionsStream stream, ChannelIdentifier channel, CancellationToken stoppingToken)
        {
            var channelMembersIds = await GetChannelMembersIdsAsync(channel, stoppingToken);

            var threads = await GetThreadsAsync(channel, context.TimeRange.Start, stoppingToken);

            foreach (var thread in threads)
            {
                var replies = await GetThreadsRepliesAsync(channel, thread, stoppingToken);
            }


            return new SingleTaskResult(0);
        }

        private async Task<List<string>> GetChannelMembersIdsAsync(ChannelIdentifier channel, CancellationToken stoppingToken)
        {
            var result = new List<string>();

            var membersResponse = await _graphClient
                .Teams[channel.TeamId]
                .Channels[channel.ChannelId]
                .Members
                .GetAsync(x =>
                {
                    x.QueryParameters = new()
                    {
                        Select = new[]
                        {
                            nameof(ConversationMember.Id)
                        }
                    };
                }, stoppingToken);

            var pageIterator = PageIterator<ConversationMember, ConversationMemberCollectionResponse>
                .CreatePageIterator(_graphClient, membersResponse,
                member =>
                {
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

            DeltaGetResponse threadsResponse = await _graphClient
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

            return result;
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

            return result;
        }
    }

    class ChannelIdentifier
    {
        public string ChannelId { get; set; }
        public string TeamId { get; set; }
    }
}
