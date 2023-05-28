using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackClientFacade : IDisposable
    {
        Task<ISet<string>> GetAllSlackChannelMembers(string slackChannelId, CancellationToken stoppingToken);
        Task<IReadOnlyCollection<ConversationsListResponse.SingleConversation>> GetAllSlackChannels(CancellationToken stoppingToken);
        Task<IEnumerable<ConversationRepliesResponse.SingleMessage>> GetAllSlackThreadReplies(string slackChannelId, string timestamp, CancellationToken stoppingToken);
        Task<IReadOnlyCollection<ConversationHistoryResponse.SingleMessage>> GetSlackThreads(string slackChannelId, TimeRange timeRange, CancellationToken stoppingToken);
        Task<IEnumerable<UsersListResponse.SingleUser>> GetAllUsers(CancellationToken stoppingToken);
        Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetCurrentUserChannels(CancellationToken stoppingToken = default);
        Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetAllUsersChannels(string slackUserId, CancellationToken stoppingToken = default);
        Task<ConversationJoinResponse> JoinChannelAsync(string conversationId, CancellationToken stoppingToken = default);
        Task<ReactionsGetResponse> GetAllReactions(string slackChannelId, string messageTimestamp, CancellationToken stoppingToken = default);
        Task<UsersInfoResponse> GetUserAsync(string id, CancellationToken stoppingToken = default);
        Task<OAuthAccessResponse> AccessAsync(OAuthAccessRequest request, CancellationToken stoppingToken = default);
    }

    internal class SlackClientFacade : ISlackClientFacade
    {
        private const int DEFAULT_LIMIT = 1000;
        private readonly ISlackHttpClient _slackHttpClient;
        private readonly CursorPaginationHandler _paginationHandler;
        private readonly ConversationsClient _conversationsClient;
        private readonly ReactionsClient _reactionsClient;
        private readonly UsersClient _usersClient;
        private readonly OAuthClient _oauthClient;

        public SlackClientFacade(ISlackHttpClient slackHttpClient, CursorPaginationHandler cursorPaginationHandler)
        {
            _slackHttpClient = slackHttpClient;
            _paginationHandler = cursorPaginationHandler;

            _conversationsClient = new ConversationsClient(_slackHttpClient);
            _reactionsClient = new ReactionsClient(_slackHttpClient);
            _usersClient = new UsersClient(_slackHttpClient);
            _oauthClient = new OAuthClient(_slackHttpClient);
        }

        public void Dispose()
        {
            _slackHttpClient?.Dispose();
        }

        public async Task<IReadOnlyCollection<ConversationsListResponse.SingleConversation>> GetAllSlackChannels(CancellationToken stoppingToken)
        {
            Task<ConversationsListResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _conversationsClient.GetListAsync(DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<ConversationsListResponse.SingleConversation> GetEntitiesFromResponse(ConversationsListResponse response)
                => response.Conversations;

            var slackChannels = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
            return slackChannels.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyCollection<ConversationHistoryResponse.SingleMessage>> GetSlackThreads(string slackChannelId, TimeRange timeRange, CancellationToken stoppingToken)
        {
            Task<ConversationHistoryResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _conversationsClient.GetConversationHistoryAsync(slackChannelId, DEFAULT_LIMIT, timeRange.Start, timeRange.End, nextCursor, stoppingToken);

            IEnumerable<ConversationHistoryResponse.SingleMessage> GetEntitiesFromResponse(ConversationHistoryResponse response)
                => response.Messages;

            var slackThreads = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
            return slackThreads
                .Where(x => x.Subtype != "channel_join")
                .ToList()
                .AsReadOnly();
        }

        public async Task<ISet<string>> GetAllSlackChannelMembers(string slackChannelId, CancellationToken stoppingToken)
        {
            Task<ConversationMembersResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _conversationsClient.GetConversationMembersAsync(slackChannelId, DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<string> GetEntitiesFromResponse(ConversationMembersResponse response)
                => response.Members;

            var slackThreadMembers = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
            return slackThreadMembers.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        }

        public async Task<IEnumerable<ConversationRepliesResponse.SingleMessage>> GetAllSlackThreadReplies(string slackChannelId, string timestamp, CancellationToken stoppingToken)
        {
            var oldest = TimeStampMapper.DateTimeToSlackTimeStamp(DateTime.UnixEpoch);
            var latest = TimeStampMapper.DateTimeToSlackTimeStamp(DateTime.MaxValue);

            Task<ConversationRepliesResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _conversationsClient.GetRepliesAsync(slackChannelId, timestamp, oldest, latest, DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<ConversationRepliesResponse.SingleMessage> GetEntitiesFromResponse(ConversationRepliesResponse response)
                => response.Messages;

            var allMessages = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);

            return allMessages
                .Where(x => x.TimeStamp != x.ThreadTimestamp); // exclude the thread message itself
        }

        public Task<ConversationJoinResponse> JoinChannelAsync(string conversationId, CancellationToken stoppingToken = default)
            => _conversationsClient.JoinConversationAsync(conversationId, stoppingToken);

        public async Task<IEnumerable<UsersListResponse.SingleUser>> GetAllUsers(CancellationToken stoppingToken)
        {
            Task<UsersListResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _usersClient.GetListAsync(DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<UsersListResponse.SingleUser> GetEntitiesFromResponse(UsersListResponse response)
                => response.Members;

            return await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
        }

        public Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetCurrentUserChannels(CancellationToken stoppingToken = default)
            => GetAllUsersChannels(string.Empty, stoppingToken);

        public async Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetAllUsersChannels(string slackUserId, CancellationToken stoppingToken = default)
        {
            Task<UsersConversationsResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _usersClient.GetConversationsAsync(slackUserId, DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<UsersConversationsResponse.SingleConversation> GetEntitiesFromResponse(UsersConversationsResponse response)
                => response.Conversations;

            return await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
        }

        public Task<ReactionsGetResponse> GetAllReactions(string slackChannelId, string messageTimestamp, CancellationToken stoppingToken = default)
            => _reactionsClient.GetAsync(slackChannelId, messageTimestamp, stoppingToken);

        public Task<UsersInfoResponse> GetUserAsync(string id, CancellationToken stoppingToken = default)
            => _usersClient.GetAsync(id, stoppingToken);

        public Task<OAuthAccessResponse> AccessAsync(OAuthAccessRequest request, CancellationToken stoppingToken = default)
            => _oauthClient.AccessAsync(request, stoppingToken);
    }
}