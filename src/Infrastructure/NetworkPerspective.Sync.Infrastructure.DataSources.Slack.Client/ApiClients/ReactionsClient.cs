using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.ApiClients
{
    internal class ReactionsClient
    {
        private readonly ISlackHttpClient _client;

        public ReactionsClient(ISlackHttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Double check this... not tested
        /// <see href="https://api.slack.com/methods/reactions.get"/>
        /// </summary>
        public async Task<ReactionsGetResponse> GetAsync(string channel, string messageTimestamp, CancellationToken stoppingToken = default)
        {
            var url = string.Format("reactions.get?full=true&channel={0}&timestamp={1}", channel, messageTimestamp);

            return await _client.GetAsync<ReactionsGetResponse>(url, stoppingToken);
        }
    }
}