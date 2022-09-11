using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    internal class TimeRangeInteractionCriteria : IInteractionCritieria
    {
        private readonly TimeRange _timeRange;
        private readonly ILogger<TimeRangeInteractionCriteria> _logger;

        public TimeRangeInteractionCriteria(TimeRange timeRange, ILogger<TimeRangeInteractionCriteria> logger)
        {
            _timeRange = timeRange;
            _logger = logger;
        }

        public IEnumerable<Interaction> MeetCriteria(IEnumerable<Interaction> input)
        {
            _logger.LogDebug("Filtering out interactions outside timerange {timeRange}. Input has {count} interactions", _timeRange, input.Count());

            var result = input
                .Where(x => _timeRange.IsInRange(x.Timestamp));

            _logger.LogDebug("Filtering interactions outside timerange completed. Output has {count} interactions", result.Count());

            return result;
        }
    }
}