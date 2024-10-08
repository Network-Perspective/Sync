﻿using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.ApiClients
{
    internal class AuthClient
    {
        private readonly ISlackHttpClient _client;

        public AuthClient(ISlackHttpClient client)
        {
            _client = client;
        }

        /// <see href="https://api.slack.com/methods/auth.teams.list"/>
        public async Task<TeamsListResponse> GetTeamsListAsync(int limit = 100, string cursor = default, CancellationToken stoppingToken = default)
        {
            var path = string.Format("auth.teams.list?limit={0}&cursor={1}", limit, cursor);

            return await _client.GetAsync<TeamsListResponse>(path, stoppingToken);
        }

        /// <see href="https://api.slack.com/methods/auth.test"/>
        public async Task<TestResponse> TestAsync(CancellationToken stoppingToken = default)
        {
            const string path = "auth.test";

            return await _client.PostAsync<TestResponse>(path, stoppingToken);
        }
    }
}