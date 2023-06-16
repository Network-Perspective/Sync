using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.ApiClients
{
    internal class AdminConversationsClient
    {
        private readonly ISlackHttpClient _client;

        public AdminConversationsClient(ISlackHttpClient client)
        {
            _client = client;
        }

        /// <see href="https://api.slack.com/methods/admin.conversations.search"/>
        public async Task<AdminConversationsListResponse> GetPrivateChannelsListAsync(string cursor = default, CancellationToken stoppingToken = default)
        {
            const int limit = 20;
            var searchTypes = "private";
            var path = string.Format("admin.conversations.search?limit={0}&cursor={1}&search_channel_types={2}", limit, cursor, searchTypes);

            return await _client.PostAsync<AdminConversationsListResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/admin.conversations.invite"/>
        public async Task<AdminConversationInvite> JoinAsync(string channelId, string userId, CancellationToken stoppingToken = default)
        {
            var path = string.Format("admin.conversations.invite?channel_id={0}&user_ids={1}", channelId, userId);

            return await _client.PostAsync<AdminConversationInvite>(path, stoppingToken);
        }
    }
}
