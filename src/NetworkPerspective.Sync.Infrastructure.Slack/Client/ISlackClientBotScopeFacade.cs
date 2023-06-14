using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.ApiClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Mappers;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackClientBotScopeFacade : IDisposable
    {
        Task<ISet<string>> GetAllSlackChannelMembersAsync(string slackChannelId, CancellationToken stoppingToken);
        Task<IReadOnlyCollection<ConversationsListResponse.SingleConversation>> GetPublicSlackChannelsAsync(CancellationToken stoppingToken);
        Task<IEnumerable<ConversationRepliesResponse.SingleMessage>> GetAllSlackThreadRepliesAsync(string slackChannelId, string timestamp, CancellationToken stoppingToken);
        Task<IReadOnlyCollection<ConversationHistoryResponse.SingleMessage>> GetSlackThreadsAsync(string slackChannelId, TimeRange timeRange, CancellationToken stoppingToken);
        Task<IEnumerable<UsersListResponse.SingleUser>> GetAllUsersAsync(CancellationToken stoppingToken);
        Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetCurrentUserChannelsAsync(CancellationToken stoppingToken = default);
        Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetAllUsersChannelsAsync(string slackUserId, CancellationToken stoppingToken = default);
        Task<IEnumerable<TeamsListResponse.SingleTeam>> GetTeamsListAsync(CancellationToken stoppingToken = default);
        Task<ConversationJoinResponse> JoinChannelAsync(string conversationId, CancellationToken stoppingToken = default);
        Task<ReactionsGetResponse> GetAllReactionsAsync(string slackChannelId, string messageTimestamp, CancellationToken stoppingToken = default);
        Task<UsersInfoResponse> GetUserAsync(string id, CancellationToken stoppingToken = default);
    }

    internal class SlackClientBotScopeFacade : ISlackClientBotScopeFacade
    {
        private const int DEFAULT_LIMIT = 1000;
        private readonly ISlackHttpClient _slackHttpClient;
        private readonly CursorPaginationHandler _paginationHandler;
        private readonly ConversationsClient _conversationsClient;
        private readonly ReactionsClient _reactionsClient;
        private readonly UsersClient _usersClient;
        private readonly AuthClient _authClient;

        public SlackClientBotScopeFacade(ISlackHttpClient slackHttpClient, CursorPaginationHandler paginationHandler)
        {
            _slackHttpClient = slackHttpClient;
            _paginationHandler = paginationHandler;

            _conversationsClient = new ConversationsClient(_slackHttpClient);
            _reactionsClient = new ReactionsClient(_slackHttpClient);
            _usersClient = new UsersClient(_slackHttpClient);
            _authClient = new AuthClient(_slackHttpClient);
        }

        public void Dispose()
        {
            _slackHttpClient?.Dispose();
        }

        public async Task<IReadOnlyCollection<ConversationsListResponse.SingleConversation>> GetPublicSlackChannelsAsync(CancellationToken stoppingToken)
        {
            Task<ConversationsListResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _conversationsClient.GetListAsync(DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<ConversationsListResponse.SingleConversation> GetEntitiesFromResponse(ConversationsListResponse response)
                => response.Conversations;

            var slackChannels = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
            return slackChannels.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyCollection<ConversationHistoryResponse.SingleMessage>> GetSlackThreadsAsync(string slackChannelId, TimeRange timeRange, CancellationToken stoppingToken)
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

        public async Task<ISet<string>> GetAllSlackChannelMembersAsync(string slackChannelId, CancellationToken stoppingToken)
        {
            Task<ConversationMembersResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _conversationsClient.GetConversationMembersAsync(slackChannelId, DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<string> GetEntitiesFromResponse(ConversationMembersResponse response)
                => response.Members;

            var slackThreadMembers = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
            return slackThreadMembers.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        }

        public async Task<IEnumerable<ConversationRepliesResponse.SingleMessage>> GetAllSlackThreadRepliesAsync(string slackChannelId, string timestamp, CancellationToken stoppingToken)
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

        public async Task<IEnumerable<UsersListResponse.SingleUser>> GetAllUsersAsync(CancellationToken stoppingToken)
        {
            Task<UsersListResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _usersClient.GetListAsync(DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<UsersListResponse.SingleUser> GetEntitiesFromResponse(UsersListResponse response)
                => response.Members;

            return await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
        }

        public Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetCurrentUserChannelsAsync(CancellationToken stoppingToken = default)
            => GetAllUsersChannelsAsync(string.Empty, stoppingToken);

        public async Task<IEnumerable<UsersConversationsResponse.SingleConversation>> GetAllUsersChannelsAsync(string slackUserId, CancellationToken stoppingToken = default)
        {
            Task<UsersConversationsResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _usersClient.GetConversationsAsync(slackUserId, DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<UsersConversationsResponse.SingleConversation> GetEntitiesFromResponse(UsersConversationsResponse response)
                => response.Conversations;

            return await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
        }

        public Task<ReactionsGetResponse> GetAllReactionsAsync(string slackChannelId, string messageTimestamp, CancellationToken stoppingToken = default)
            => _reactionsClient.GetAsync(slackChannelId, messageTimestamp, stoppingToken);

        public Task<UsersInfoResponse> GetUserAsync(string id, CancellationToken stoppingToken = default)
            => _usersClient.GetAsync(id, stoppingToken);

        public async Task<IEnumerable<TeamsListResponse.SingleTeam>> GetTeamsListAsync(CancellationToken stoppingToken = default)
        {
            Task<TeamsListResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _authClient.GetTeamsListAsync(DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<TeamsListResponse.SingleTeam> GetEntitiesFromResponse(TeamsListResponse response)
                => response.Teams;

            return await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
        }
    }
}