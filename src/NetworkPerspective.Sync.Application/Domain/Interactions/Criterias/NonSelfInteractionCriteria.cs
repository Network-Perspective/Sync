using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Application.Domain.Interactions.Criterias
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
            _logger.LogTrace("Filtering out self interactions. Input has {count} interactions", input.Count());

            var result = input
                .Where(x => !EmployeeId.EqualityComparer.Equals(x.Source.Id, x.Target.Id));

            _logger.LogTrace("Filtering self interactions completed. Output has {count} interactions", result.Count());

            return result;
        }
    }
}