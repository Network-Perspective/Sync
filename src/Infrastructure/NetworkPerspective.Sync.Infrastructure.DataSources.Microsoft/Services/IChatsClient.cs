using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using InternalChat = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Chat;
using InternalChatMessageReaction = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.ChatMessageReaction;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services
{
    internal interface IChatsClient
    {
        Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IChatInteractionFactory interactionFactory, CancellationToken stoppingToken = default);
    }

    internal class ChatsClient : IChatsClient
    {
        private const string TaskCaption = "Synchronizing chat interactions";
        private const string TaskDescription = "Fetching chat metadata from Microsoft API";

        private readonly GraphServiceClient _graphClient;
        private readonly IGlobalStatusCache _tasksStatusesCache;
        private readonly ILogger<ChatsClient> _logger;

        public ChatsClient(GraphServiceClient graphClient, IGlobalStatusCache tasksStatusesCache, ILogger<ChatsClient> logger)
        {
            _graphClient = graphClient;
            _tasksStatusesCache = tasksStatusesCache;
            _logger = logger;
        }

        public async Task<SyncResult> SyncInteractionsAsync(SyncContext context, IInteractionsStream stream, IEnumerable<string> usersEmails, IChatInteractionFactory interactionFactory, CancellationToken stoppingToken = default)
        {
            async Task ReportProgressCallbackAsync(double progressRate)
            {
                var taskStatus = SingleTaskStatus.New(TaskCaption, TaskDescription, progressRate);
                await _tasksStatusesCache.SetStatusAsync(context.ConnectorId, taskStatus, stoppingToken);
            }

            Task<SingleTaskResult> SingleTaskAsync(InternalChat chat)
                => SyncChatInteractionsAsync(context, stream, chat, interactionFactory, stoppingToken);

            var chats = await GetAllChatsAsync(usersEmails, context.TimeRange, stoppingToken);
            _logger.LogInformation("Evaluating interactions based on chat for '{timerange}' for {count} users...", context.TimeRange, usersEmails.Count());
            var result = await ParallelSyncTask<InternalChat>.RunAsync(chats, ReportProgressCallbackAsync, SingleTaskAsync, stoppingToken);
            _logger.LogInformation("Evaluation of interactions based on chat for '{timerange}' completed", context.TimeRange);

            return result;
        }

        public async Task<IEnumerable<InternalChat>> GetAllChatsAsync(IEnumerable<string> usersEmails, Utils.Models.TimeRange timeRange, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Getting all Chats of all Employees...");

            var result = new List<InternalChat>();

            foreach (var email in usersEmails)
            {
                var chats = await GetSingleUserChatsAsync(email, timeRange, stoppingToken);
                result.AddRange(chats);
            }

            var uniqueChats = result.DistinctBy(chat => chat.Id);

            _logger.LogDebug("Got {count} chats", uniqueChats.Count());

            return uniqueChats;
        }

        // https://learn.microsoft.com/en-us/graph/api/chat-list
        private async Task<IEnumerable<InternalChat>> GetSingleUserChatsAsync(string userEmail, Utils.Models.TimeRange timeRange, CancellationToken stoppingToken)
        {
            const int maxPageSize = 50;
            var filterString = $"lastUpdatedDateTime ge {timeRange.Start:s}Z and lastUpdatedDateTime lt {timeRange.End:s}Z";

            var chats = await _graphClient
                    .Users[userEmail]
                    .Chats
                    .GetAsync(x =>
                    {
                        x.QueryParameters = new()
                        {
                            Expand = new[]
                            {
                                nameof(Chat.Members)
                            },
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

            var messagesResponse = await _graphClient
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

            var pageIterator = PageIterator<ChatMessage, ChatMessageCollectionResponse>
                .CreatePageIterator(_graphClient, messagesResponse,
                async message =>
                {
                    if (message.MessageType == ChatMessageType.Message)
                    {
                        var chatMessage = ChatMessageMapper.ToInternalChatMessage(message, chat);
                        var chatMessageInteractions = interactionFactory.CreateFromChatMessage(chatMessage);
                        var chatMessageInteractionsCount = await stream.SendAsync(chatMessageInteractions);
                        interactionsCount += chatMessageInteractionsCount;

                        var reactions = message.Reactions is null
                            ? Enumerable.Empty<InternalChatMessageReaction>()
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
}