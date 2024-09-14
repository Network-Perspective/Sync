using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services
{
    internal class ChannelFilter
    {
        private readonly ILogger<ChannelFilter> _logger;

        public ChannelFilter(ILogger<ChannelFilter> logger)
        {
            _logger = logger;
        }

        public List<Channel> Filter(IEnumerable<Channel> channels, EmployeeFilter emailFilter)
        {
            _logger.LogDebug("Filtering {count} channels.. Allowing only channels with at least one 'white-listed' member", channels.Count());

            var result = channels
                .Where(channel => channel.UserIds.Any(emailFilter.IsInternal))
                .ToList();

            _logger.LogDebug("Filtering completed. Allowed {resultCount} out of {inputCount}", result.Count, channels.Count());

            return result;
        }
    }
}