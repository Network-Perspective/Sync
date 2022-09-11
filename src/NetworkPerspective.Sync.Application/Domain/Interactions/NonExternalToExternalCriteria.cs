using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    internal class NonExternalToExternalCriteria : IInteractionCritieria
    {
        private readonly ILogger<NonExternalToExternalCriteria> _logger;

        public NonExternalToExternalCriteria(ILogger<NonExternalToExternalCriteria> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Interaction> MeetCriteria(IEnumerable<Interaction> input)
        {
            _logger.LogDebug("Filtering out external to external interactions. Input has {count} interactions", input.Count());

            var result = input
                .Where(x => !(x.Source.IsExternal && x.Target.IsExternal));

            _logger.LogDebug("Filtering bot interactions completed. Output has {count} interactions", result.Count());

            return result;
        }
    }
}