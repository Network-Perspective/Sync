using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NetworkPerspective.Sync.Infrastructure.Core;

namespace NetworkPerspective.Sync.RegressionTests.Interactions
{
    internal class HashedInteractionEqualityComparer : IEqualityComparer<HashedInteraction>
    {
        private readonly IEqualityComparer<IDictionary<string, string>> _vertexEqualityComparer = new VertexIdEqualityComparer();

        public bool Equals(HashedInteraction x, HashedInteraction y)
        {
            if (x == null || y == null)
                return false;

            //if (x.InteractionId != y.InteractionId)
            //    return false;

            if (x.When != y.When)
                return false;

            if (!_vertexEqualityComparer.Equals(x.SourceIds, y.SourceIds))
                return false;

            if (!_vertexEqualityComparer.Equals(x.TargetIds, y.TargetIds))
                return false;

            if (x.EventId != y.EventId)
                return false;

            //if (x.ParentEventId != y.ParentEventId)
            //    return false;

            //if (x.ChannelId != y.ChannelId)
            //    return false;

            //if (x.DurationMinutes != y.DurationMinutes)
            //    return false;

            if (!x.Label.ToHashSet().SetEquals(y.Label.ToHashSet()))
                return false;

            return true;
        }

        public int GetHashCode([DisallowNull] HashedInteraction obj)
        {
            var hashCode = new HashCode();

            //hashCode.Add(obj.InteractionId ?? string.Empty);
            hashCode.Add(obj.When);
            hashCode.Add(_vertexEqualityComparer.GetHashCode(obj.SourceIds));
            hashCode.Add(_vertexEqualityComparer.GetHashCode(obj.TargetIds));
            hashCode.Add(obj.EventId ?? string.Empty);
            //hashCode.Add(obj.ParentEventId ?? string.Empty);
            //hashCode.Add(obj.ChannelId ?? string.Empty);
            //hashCode.Add(obj.DurationMinutes);

            foreach (var label in obj.Label)
                hashCode.Add(label);

            return hashCode.ToHashCode();
        }
    }
}
