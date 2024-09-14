using System.Collections.Generic;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Interactions.Criterias
{
    internal interface IInteractionCritieria
    {
        IEnumerable<Interaction> MeetCriteria(IEnumerable<Interaction> input);
    }
}