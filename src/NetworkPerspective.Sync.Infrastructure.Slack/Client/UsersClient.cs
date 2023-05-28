﻿using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal class UsersClient
    {
        private readonly ISlackHttpClient _client;

        public UsersClient(ISlackHttpClient client)
        {
            _client = client;
        }

        /// <see href="https://api.slack.com/methods/users.list"
        public async Task<UsersListResponse> GetListAsync(int limit = 100, string cursor = default, CancellationToken stoppingToken = default)
        {
            var path = string.Format("users.list?limit={0}&cursor={1}", limit, cursor);

            return await _client.GetAsync<UsersListResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/users.info"
        public async Task<UsersInfoResponse> GetAsync(string id, CancellationToken stoppingToken = default)
        {
            var path = string.Format("users.info?user={0}", id);

            return await _client.GetAsync<UsersInfoResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/users.conversations"
        public async Task<UsersConversationsResponse> GetConversationsAsync(string userId, int limit = 100, string cursor = default, CancellationToken stoppingToken = default)
        {
            var conversationTypes = "public_channel,private_channel";
            var user = string.IsNullOrEmpty(userId) ? string.Empty : $"&user={userId}";
            var path = string.Format("users.conversations?&limit={0}&types={1}&cursor={2}{3}", limit, conversationTypes, cursor, user);

            return await _client.GetAsync<UsersConversationsResponse>(path, stoppingToken);
        }
    }
}