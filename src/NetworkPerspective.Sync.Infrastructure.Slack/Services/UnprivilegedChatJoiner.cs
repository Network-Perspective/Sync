using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Slack.Client;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class UnprivilegedChatJoiner : IChatJoiner
    {
        private readonly ISlackClientBotScopeFacade _slackClientBotScope;
        private readonly ILogger<UnprivilegedChatJoiner> _logger;

        public UnprivilegedChatJoiner(ISlackClientBotScopeFacade slackClientBotScope, ILogger<UnprivilegedChatJoiner> logger)
        {
            _slackClientBotScope = slackClientBotScope;
            _logger = logger;
        }

        public async Task JoinAsync(CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Joining public channels...");

            var teams = await _slackClientBotScope.GetTeamsListAsync(stoppingToken);

            foreach (var team in teams)
            {
                var channels = await _slackClientBotScope.GetSlackChannelsAsync(team.Id, stoppingToken);

                foreach (var channel in channels.Where(x => !x.IsPrivate))
                    await _slackClientBotScope.JoinChannelAsync(channel.Id, stoppingToken);
            }

            _logger.LogDebug("Joining public channels completed");
        }
    }
}