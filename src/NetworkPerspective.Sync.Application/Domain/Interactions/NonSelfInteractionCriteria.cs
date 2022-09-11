using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    public class NonSelfInteractionCriteria : IInteractionCritieria
    {
        private readonly ILogger<NonSelfInteractionCriteria> _logger;

        public NonSelfInteractionCriteria(ILogger<NonSelfInteractionCriteria> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Interaction> MeetCriteria(IEnumerable<Interaction> input)
        {
            _logger.LogDebug("Filtering out self interactions. Input has {count} interactions", input.Count());

            var result = input
                .Where(x => !Consts.UserIdEqualityComparer.Equals(x.Source.Email, x.Target.Email));

            _logger.LogDebug("Filtering self interactions completed. Output has {count} interactions", result.Count());

            return result;
        }
    }
}