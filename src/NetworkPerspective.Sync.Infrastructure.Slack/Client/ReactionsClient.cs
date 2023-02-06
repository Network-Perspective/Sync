using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal class ReactionsClient : ApiClientBase
    {
        public ReactionsClient(HttpClient client, ILogger<ReactionsClient> logger) : base(client, logger)
        { }

        /// <summary>
        /// Double check this... not tested
        /// <see href="https://api.slack.com/methods/reactions.get"/>
        /// </summary>
        public async Task<ReactionsGetResponse> GetAsync(string channel, string messageTimestamp, CancellationToken stoppingToken = default)
        {
            var url = string.Format("reactions.get?full=true&channel={0}&timestamp={1}", channel, messageTimestamp);

            return await Get<ReactionsGetResponse>(url, stoppingToken);
        }
    }
}