using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    public class InteractionEqualityComparer : IEqualityComparer<Interaction>
    {
        private readonly IEqualityComparer<InteractionVertex> _interactionVertexEqualityComparer;

        public InteractionEqualityComparer(IEqualityComparer<InteractionVertex> interactionVertexEqualityComparer)
        {
            _interactionVertexEqualityComparer = interactionVertexEqualityComparer;
        }

        public bool Equals(Interaction x, Interaction y)
        {
            if (x == null || y == null)
                return false;

            if (x.Timestamp != y.Timestamp)
                return false;

            if (!_interactionVertexEqualityComparer.Equals(x.Source, y.Source))
                return false;

            if (!_interactionVertexEqualityComparer.Equals(x.Target, y.Target))
                return false;

            if (!string.Equals(x.ChannelId, y.ChannelId))
                return false;

            if (x.Type != y.Type)
                return false;

            if (x.UserAction == null && y.UserAction != null)
                return false;

            if (x.UserAction != null && y.UserAction == null)
                return false;

            if (x.UserAction != null && y.UserAction != null)
            {
                if (!x.UserAction.SetEquals(y.UserAction))
                    return false;
            }

            return true;
        }

        public int GetHashCode(Interaction obj)
        {
            var userActionHash = new HashCode();

            foreach (var item in obj.UserAction)
                userActionHash.Add(item);

            return HashCode.Combine(obj.Timestamp, _interactionVertexEqualityComparer.GetHashCode(obj.Source), _interactionVertexEqualityComparer.GetHashCode(obj.Target), obj.Type, userActionHash.ToHashCode(), obj.ChannelId);
        }
    }
}