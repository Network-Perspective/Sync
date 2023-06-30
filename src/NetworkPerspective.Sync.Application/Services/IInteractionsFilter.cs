using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Interactions.Criterias;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IInteractionsFilter
    {
        public ISet<Interaction> Filter(IEnumerable<Interaction> interactions);
    }

    internal class InteractionsFilter : IInteractionsFilter
    {
        private readonly IEnumerable<IInteractionCritieria> _interactionCritierias;
        private readonly ILogger<IInteractionsFilter> _logger;

        public InteractionsFilter(IEnumerable<IInteractionCritieria> interactionCritierias, ILogger<IInteractionsFilter> logger)
        {
            _interactionCritierias = interactionCritierias;
            _logger = logger;
        }

        public ISet<Interaction> Filter(IEnumerable<Interaction> interactions)
        {
            _logger.LogTrace("Filtering {count} interactions", interactions.Count());

            foreach (var criteria in _interactionCritierias)
                interactions = criteria.MeetCriteria(interactions);

            _logger.LogTrace("After filtering there are {count} interactions", interactions.Count());

            return interactions.ToHashSet(new InteractionEqualityComparer());
        }
    }
}