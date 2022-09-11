using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal class ConversationsClient : ApiClientBase
    {
        public ConversationsClient(HttpClient client) : base(client)
        { }

        /// <see href="https://api.slack.com/methods/conversations.list"
        public async Task<ConversationsListResponse> GetListAsync(int limit = 100, string cursor = default, CancellationToken stoppingToken = default)
        {
            var conversationTypes = "public_channel,private_channel";
            var path = string.Format("conversations.list?limit={0}&types={1}&cursor={2}", limit, conversationTypes, cursor);

            return await Get<ConversationsListResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.members"/>
        public async Task<ConversationMembersResponse> GetConversationMembersAsync(string conversationId, int limit, string cursor = default, CancellationToken stoppingToken = default)
        {
            var path = string.Format("conversations.members?channel={0}&limit={1}&cursor={2}", conversationId, limit, cursor);

            return await Get<ConversationMembersResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.history"/>
        public async Task<ConversationHistoryResponse> GetConversationHistoryAsync(string conversationId, int limit, DateTime oldest, DateTime latest, string cursor = default, CancellationToken stoppingToken = default)
        {
            var notOlderThan = TimeStampMapper.DateTimeToSlackTimeStamp(oldest);
            var notNewerThan = TimeStampMapper.DateTimeToSlackTimeStamp(latest);
            var path = string.Format("conversations.history?channel={0}&inclusive=true&limit={1}&oldest={2}&latest{3}&cursor={4}", conversationId, limit, notOlderThan, notNewerThan, cursor);

            return await Get<ConversationHistoryResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.join"/>
        public async Task<JoinConversationResponse> JoinConversationAsync(string conversationId, CancellationToken stoppingToken = default)
        {
            var path = string.Format("conversations.join?channel={0}", conversationId);

            return await Post<JoinConversationResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/conversations.replies"/>
        public async Task<ConversationRepliesResponse> GetRepliesAsync(string conversationId, string timestamp, string oldest, string latest, int limit, string cursor = default, CancellationToken stoppingToken = default)
        {
            var path = string.Format("conversations.replies?channel={0}&ts={1}&oldest={2}&latest={3}&limit={4}&cursor={5}", conversationId, timestamp, oldest, latest, limit, cursor);

            return await Get<ConversationRepliesResponse>(path, stoppingToken);
        }
    }
}