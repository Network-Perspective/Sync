using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using System.Linq;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.ApiClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal interface ISlackClientUserScopeFacade : IDisposable
    {
        Task<IReadOnlyCollection<ConversationsListResponse.SingleConversation>> GetPrivateSlackChannelsAsync(CancellationToken stoppingToken = default);
        Task<AdminConversationInvite> JoinChannelAsync(string conversationId, string userId, CancellationToken stoppingToken = default);
    }

    internal class SlackClientUserScopeFacade : ISlackClientUserScopeFacade
    {
        private const int DEFAULT_LIMIT = 1000;
        private readonly ISlackHttpClient _slackHttpClient;
        private readonly CursorPaginationHandler _paginationHandler;
        private readonly AdminConversationsClient _adminConversationsClient;

        public SlackClientUserScopeFacade(ISlackHttpClient slackHttpClient, CursorPaginationHandler paginationHandler)
        {
            _slackHttpClient = slackHttpClient;
            _paginationHandler = paginationHandler;
            _adminConversationsClient = new AdminConversationsClient(_slackHttpClient);
        }

        public void Dispose()
        {
            _slackHttpClient.Dispose();
        }

        public async Task<IReadOnlyCollection<ConversationsListResponse.SingleConversation>> GetPrivateSlackChannelsAsync(CancellationToken stoppingToken = default)
        {
            Task<ConversationsListResponse> CallApi(string nextCursor, CancellationToken stoppingToken)
                => _adminConversationsClient.GetPrivateChannelsListAsync(DEFAULT_LIMIT, nextCursor, stoppingToken);

            IEnumerable<ConversationsListResponse.SingleConversation> GetEntitiesFromResponse(ConversationsListResponse response)
                => response.Conversations;

            var slackChannels = await _paginationHandler.GetAllAsync(CallApi, GetEntitiesFromResponse, stoppingToken);
            return slackChannels.ToList().AsReadOnly();
        }
        public Task<AdminConversationInvite> JoinChannelAsync(string conversationId, string userId, CancellationToken stoppingToken = default)
            => _adminConversationsClient.JoinAsync(conversationId, userId, stoppingToken);
    }
}
