using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Interactions.Criterias
{
    internal class NonBotInteractionCriteria : IInteractionCritieria
    {
        private readonly ILogger<NonBotInteractionCriteria> _logger;

        public NonBotInteractionCriteria(ILogger<NonBotInteractionCriteria> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Interaction> MeetCriteria(IEnumerable<Interaction> input)
        {
            _logger.LogTrace("Filtering out bot interactions. Input has {count} interactions", input.Count());

            var result = input
                .Where(x => !x.Source.IsBot)
                .Where(x => !x.Target.IsBot);

            _logger.LogTrace("Filtering bot interactions completed. Output has {count} interactions", result.Count());

            return result;
        }
    }
}