using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Interactions.Criterias
{
    internal interface IInteractionCritieria
    {
        IEnumerable<Interaction> MeetCriteria(IEnumerable<Interaction> input);
    }
}