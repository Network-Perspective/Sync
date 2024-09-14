using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.ApiClients
{
    internal class ConversationsClient
    {
        private readonly ISlackHttpClient _client;

        public ConversationsClient(ISlackHttpClient client)
        {
            _client = client;
        }

        /// <see href="https://api.slack.com/methods/conversations.list"/>
        public async Task<ConversationsListResponse> GetListAsync(string teamId = default, int limit = 100, string cursor = default, CancellationToken stoppingToken = default)
        {
            var conversationTypes = "public_channel,private_channel";
            var path = string.Format("conversations.list?team_id={0}&limit={1}&types={2}&cursor={3}", teamId, limit, conversationTypes, cursor);

            return await _client.GetAsync<ConversationsListResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.members"/>
        public async Task<ConversationMembersResponse> GetConversationMembersAsync(string conversationId, int limit, string cursor = default, CancellationToken stoppingToken = default)
        {
            var path = string.Format("conversations.members?channel={0}&limit={1}&cursor={2}", conversationId, limit, cursor);

            return await _client.GetAsync<ConversationMembersResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.history"/>
        public async Task<ConversationHistoryResponse> GetConversationHistoryAsync(string conversationId, int limit, DateTime oldest, DateTime latest, string cursor = default, CancellationToken stoppingToken = default)
        {
            var notOlderThan = TimeStampMapper.DateTimeToSlackTimeStamp(oldest);
            var notNewerThan = TimeStampMapper.DateTimeToSlackTimeStamp(latest);
            var path = string.Format("conversations.history?channel={0}&inclusive=true&limit={1}&oldest={2}&latest{3}&cursor={4}", conversationId, limit, notOlderThan, notNewerThan, cursor);

            return await _client.GetAsync<ConversationHistoryResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.join"/>
        public async Task<ConversationJoinResponse> JoinConversationAsync(string conversationId, CancellationToken stoppingToken = default)
        {
            var path = string.Format("conversations.join?channel={0}", conversationId);

            return await _client.PostAsync<ConversationJoinResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.replies"/>
        public async Task<ConversationRepliesResponse> GetRepliesAsync(string conversationId, string timestamp, string oldest, string latest, int limit, string cursor = default, CancellationToken stoppingToken = default)
        {
            var path = string.Format("conversations.replies?channel={0}&ts={1}&oldest={2}&latest={3}&limit={4}&cursor={5}", conversationId, timestamp, oldest, latest, limit, cursor);

            return await _client.GetAsync<ConversationRepliesResponse>(path, stoppingToken);
        }
    }
}