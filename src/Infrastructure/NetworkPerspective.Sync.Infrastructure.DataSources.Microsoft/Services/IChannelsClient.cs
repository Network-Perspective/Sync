using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Teams.Item.Channels.Item.Messages.Delta;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using ChatMessage = Microsoft.Graph.Models.ChatMessage;
using GraphChannel = Microsoft.Graph.Models.Channel;
using InternalChannel = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Channel;
using InternalTeam = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Team;
using MicrosoftTeam = Microsoft.Graph.Models.Team;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal interface IChannelsClient
{
    Task<SyncResult> SyncInteractionsAsync(SyncContext context, IEnumerable<InternalChannel> channels, IInteractionsStream stream, IChannelsInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    Task<List<InternalChannel>> GetAllChannelsAsync(CancellationToken stoppingToken = default);
}

internal class ChannelsClient(GraphServiceClient graphClient, IGlobalStatusCache tasksStatusesCache, IStatusLogger statusLogger, ILoggerFactory loggerFactory) : IChannelsClient
{
    private const string TaskCaption = "Synchronizing channels interactions";
    private const string TaskDescription = "Fetching channels metadata from Microsoft API";
    private readonly ILogger<ChannelsClient> _logger = loggerFactory.CreateLogger<ChannelsClient>();

    public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IEnumerable<InternalChannel> channels, IInteractionsStream stream, IChannelsInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
    {
        async Task ReportProgressCallbackAsync(double progressRate)
        {
            var taskStatus = SingleTaskStatus.New(TaskCaption, TaskDescription, progressRate);
            await tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);
        }

        Task<SingleTaskResult> SingleTaskAsync(InternalChannel channel)
            => GetSingleChannelInteractionsAsync(context, stream, channel, interactionFactory, context.TimeRange, stoppingToken);

        var filteredChannels = new ChannelFilter(loggerFactory.CreateLogger<ChannelFilter>())
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

        var teams = await TryGetAllTeamsAsync(stoppingToken);
        var tasks = teams.Select(x => TryGetAllChannelsInSingleTeamAsync(x, stoppingToken));

        var channelsResults = await Task.WhenAll(tasks);

        var channels = channelsResults
            .SelectMany(x => x.Channels)
            .ToList();

        var failedTasks = channelsResults.Where(x => x.Exception is not null);

        if (failedTasks.Any())
            await statusLogger.LogDebugAsync($"Encountered problems while retrieving chats. Couldn't list chats for {failedTasks.Count()}/{teams.Count} users", stoppingToken);

        _logger.LogDebug("Got {count} Channels", result.Count);

        return channels;
    }

    private async Task<List<InternalTeam>> TryGetAllTeamsAsync(CancellationToken stoppingToken)
    {
        try
        {
            return await GetAllTeamsAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to get teams");
            await statusLogger.LogDebugAsync($"Unable to get teams: '{ex.Message}'", stoppingToken);
            return [];
        }
    }

    private async Task<List<InternalTeam>> GetAllTeamsAsync(CancellationToken stoppingToken)
    {
        var result = new List<InternalTeam>();
        var teamsResponse = await graphClient
            .Teams
            .GetAsync(x =>
            {
                x.QueryParameters = new()
                {
                    Select =
                    [
                        nameof(MicrosoftTeam.Id),
                        nameof(MicrosoftTeam.DisplayName)
                    ]
                };
            }, stoppingToken);

        var pageIterator = PageIterator<MicrosoftTeam, TeamCollectionResponse>
            .CreatePageIterator(graphClient, teamsResponse,
            team =>
            {
                result.Add(new InternalTeam(team.Id, team.DisplayName));
                return true;
            },
            request =>
            {
                return request;
            });
        await pageIterator.IterateAsync(stoppingToken);

        return result;
    }

    private async Task<GetChannelsResult> TryGetAllChannelsInSingleTeamAsync(InternalTeam team, CancellationToken stoppingToken)
    {
        try
        {
            var channels = await GetAllChannelsInSingleTeamAsync(team, stoppingToken);
            return GetChannelsResult.Success(channels);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to get channel");
            return GetChannelsResult.Error(ex);
        }
    }

    private async Task<List<InternalChannel>> GetAllChannelsInSingleTeamAsync(InternalTeam team, CancellationToken stoppingToken)
    {
        var result = new List<InternalChannel>();
        var channelsResponse = await graphClient
            .Teams[team.Id].AllChannels
            .GetAsync(x =>
            {
                x.QueryParameters = new()
                {
                    Select =
                    [
                        nameof(GraphChannel.Id),
                        nameof(GraphChannel.DisplayName)
                    ]
                };
            }, stoppingToken);

        var pageIterator = PageIterator<GraphChannel, ChannelCollectionResponse>
            .CreatePageIterator(graphClient, channelsResponse,
            async channel =>
            {
                var userIds = await GetChannelMembersIdsAsync(team.Id, channel.Id, stoppingToken);
                var internalChannel = new InternalChannel(channel.Id, channel.DisplayName, team, userIds);

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

    private async Task<SingleTaskResult> GetSingleChannelInteractionsAsync(SyncContext context, IInteractionsStream stream, InternalChannel channel, IChannelsInteractionFactory interactionFactory, Utils.Models.TimeRange timeRange, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Getting interactions from channel...");

        var interactionsCount = 0;

        var threads = await GetThreadsAsync(channel, context.TimeRange.Start, stoppingToken);

        foreach (var thread in threads)
        {
            var threadInteractions = interactionFactory.CreateFromThreadMessage(thread, channel);
            var sentThreadInteractionsCount = await stream.SendAsync(threadInteractions);
            interactionsCount += sentThreadInteractionsCount;

            var replies = await GetThreadsRepliesAsync(channel, thread, stoppingToken);
            var repliesInteractions = interactionFactory.CreateFromThreadRepliesMessage(replies, channel.Id, thread.Id, thread.From.User.Id, timeRange);
            var sentRepliesInteractionsCount = await stream.SendAsync(repliesInteractions);
            interactionsCount += sentRepliesInteractionsCount;
        }

        _logger.LogDebug("Getting interactions from channel completed");


        return new SingleTaskResult(interactionsCount);
    }

    private async Task<ISet<string>> GetChannelMembersIdsAsync(string teamId, string channelId, CancellationToken stoppingToken)
    {
        var result = new HashSet<string>();

        var membersResponse = await graphClient
            .Teams[teamId]
            .Channels[channelId]
            .Members
            .GetAsync(cancellationToken: stoppingToken);

        var pageIterator = PageIterator<ConversationMember, ConversationMemberCollectionResponse>
            .CreatePageIterator(graphClient, membersResponse,
            member =>
            {
                result.Add(member.GetUserId());

                return true;
            },
            request =>
            {
                return request;
            });
        await pageIterator.IterateAsync(stoppingToken);

        return result;
    }
    private async Task<List<ChatMessage>> GetThreadsAsync(InternalChannel channel, DateTime from, CancellationToken stoppingToken)
    {
        var filterString = $"lastModifiedDateTime gt {from:s}Z";
        var result = new List<ChatMessage>();

        var threadsResponse = await graphClient
            .Teams[channel.Team.Id]
            .Channels[channel.Id]
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
            .CreatePageIterator(graphClient, threadsResponse,
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

    private async Task<List<ChatMessage>> GetThreadsRepliesAsync(InternalChannel channel, ChatMessage thread, CancellationToken stoppingToken)
    {
        var result = new List<ChatMessage>();

        var threadsRepliesResponse = await graphClient
            .Teams[channel.Team.Id]
            .Channels[channel.Id]
            .Messages[thread.Id]
            .Replies
            .GetAsync(x =>
            {

            }, stoppingToken);

        var pageIterator = PageIterator<ChatMessage, ChatMessageCollectionResponse>
            .CreatePageIterator(graphClient, threadsRepliesResponse,
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