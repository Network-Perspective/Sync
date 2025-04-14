using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using InternalChat = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Chat;
using MicrosoftChat = Microsoft.Graph.Models.Chat;
using MicrosoftChatMessage = Microsoft.Graph.Models.ChatMessage;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;

internal interface IChatsClient
{
    Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IChatInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
}

internal class ChatsClient(GraphServiceClient graphClient, IGlobalStatusCache tasksStatusesCache, IStatusLogger statusLogger, ILogger<ChatsClient> logger) : IChatsClient
{
    private const string TaskCaption = "Synchronizing chat interactions";
    private const string TaskDescription = "Fetching chat metadata from Microsoft API";

    public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IChatInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
    {
        async Task ReportProgressCallbackAsync(double progressRate)
        {
            var taskStatus = SingleTaskStatus.New(TaskCaption, TaskDescription, progressRate);
            await tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);
        }

        Task<SingleTaskResult> SingleTaskAsync(InternalChat chat)
            => SyncChatInteractionsAsync(context, stream, chat, interactionFactory, stoppingToken);

        var chats = await GetAllChatsAsync(usersEmails, context.TimeRange, stoppingToken);
        logger.LogInformation("Evaluating interactions based on chat for '{timerange}' for {count} users...", context.TimeRange, usersEmails.Count());
        var result = await ParallelSyncTask<InternalChat>.RunAsync(chats, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
        logger.LogInformation("Evaluation of interactions based on chat for '{timerange}' completed", context.TimeRange);

        return result;
    }

    private async Task<IEnumerable<InternalChat>> GetAllChatsAsync(IEnumerable<string> usersEmails, Utils.Models.TimeRange timeRange, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Getting all Chats of all Employees...");

        var tasks = usersEmails
            .Select(x => TryGetSingleUserChatsAsync(x, timeRange, stoppingToken));

        var chatsResults = await Task.WhenAll(tasks);

        var uniqueChats = chatsResults
            .SelectMany(x => x.Chats)   // Flatten
            .DistinctBy(x => x.Id)      // Unique chats
            .ToList();

        var failedTasks = chatsResults.Where(x => x.Exception is not null);

        if (failedTasks.Any())
            await statusLogger.LogDebugAsync($"Encountered problems while retrieving chats. Couldn't list chats for {failedTasks.Count()}/{usersEmails.Count()} users", stoppingToken);

        logger.LogDebug("Got {count} chats", uniqueChats.Count);

        return uniqueChats;
    }

    private async Task<GetChatsResults> TryGetSingleUserChatsAsync(string userEmail, Utils.Models.TimeRange timeRange, CancellationToken stoppingToken)
    {
        try
        {
            var chats = await GetSingleUserChatsAsync(userEmail, timeRange, stoppingToken);
            return GetChatsResults.Success(chats);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to get chat from a user");
            return GetChatsResults.Error(ex);
        }
    }

    // https://learn.microsoft.com/en-us/graph/api/chat-list
    private async Task<IEnumerable<InternalChat>> GetSingleUserChatsAsync(string userEmail, Utils.Models.TimeRange timeRange, CancellationToken stoppingToken)
    {
        const int maxPageSize = 50;
        var filterString = $"lastUpdatedDateTime ge {timeRange.Start:s}Z and lastUpdatedDateTime lt {timeRange.End:s}Z";

        var chats = await graphClient
                .Users[userEmail]
                .Chats
                .GetAsync(x =>
                {
                    x.QueryParameters = new()
                    {
                        Expand =
                        [
                            nameof(MicrosoftChat.Members)
                        ],
                        Filter = filterString,
                        Top = maxPageSize
                    };
                }, stoppingToken);

        return chats.Value.Select(ChatMapper.ToInternalChat);
    }

    // https://learn.microsoft.com/en-us/graph/api/chat-list-messages
    private async Task<SingleTaskResult> SyncChatInteractionsAsync(SyncContext context, IInteractionsStream stream, InternalChat chat, IChatInteractionFactory interactionFactory, CancellationToken stoppingToken)
    {
        const int maxPageSize = 50;
        var filterString = $"lastModifiedDateTime gt {context.TimeRange.Start:s}Z and lastModifiedDateTime lt {context.TimeRange.End:s}Z";

        var interactionsCount = 0;

        var messagesResponse = await graphClient
            .Chats[chat.Id]
            .Messages
            .GetAsync(x =>
            {
                x.QueryParameters = new()
                {
                    // Select is not supported by the endpoint
                    Filter = filterString,
                    Top = maxPageSize
                };
            }, stoppingToken);

        var pageIterator = PageIterator<MicrosoftChatMessage, ChatMessageCollectionResponse>
            .CreatePageIterator(graphClient, messagesResponse,
            async message =>
            {
                if (message.MessageType == ChatMessageType.Message)
                {
                    var chatMessage = ChatMessageMapper.ToInternalChatMessage(message, chat);
                    var chatMessageInteractions = interactionFactory.CreateFromChatMessage(chatMessage);
                    var chatMessageInteractionsCount = await stream.SendAsync(chatMessageInteractions);
                    interactionsCount += chatMessageInteractionsCount;

                    var reactions = message.Reactions is null
                        ? []
                        : message.Reactions.Select(x => ChatMessageReactionMapper.ToInternalChatMessageReaction(x, chatMessage));

                    foreach (var reaction in reactions)
                    {
                        var reactionInteractions = interactionFactory.CreateFromChatMessageReaction(reaction);
                        var reactionInteractionsCount = await stream.SendAsync(reactionInteractions);
                        interactionsCount += reactionInteractionsCount;
                    }
                }
                return true;
            },
            request =>
            {
                return request;
            });
        await pageIterator.IterateAsync(stoppingToken);

        return new SingleTaskResult(interactionsCount);
    }
}