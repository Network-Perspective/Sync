using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services
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

            var successfulJoins = 0;
            var failedJoins = 0;
            foreach (var team in teams)
            {
                var channels = await _slackClientBotScope.GetSlackChannelsAsync(team.Id, stoppingToken);

                foreach (var channel in channels.Where(x => !x.IsPrivate))
                {
                    try
                    {
                        await _slackClientBotScope.JoinChannelAsync(channel.Id, stoppingToken);
                        successfulJoins++;
                    }
                    catch (Exception)
                    {
                        failedJoins++;
                    }
                }
            }

            _logger.LogInformation("Successfully joined {0} channels", successfulJoins);
            if (failedJoins > 0) _logger.LogInformation("Failed to join {0} channels", failedJoins);

            _logger.LogDebug("Joining public channels completed");
        }
    }
}