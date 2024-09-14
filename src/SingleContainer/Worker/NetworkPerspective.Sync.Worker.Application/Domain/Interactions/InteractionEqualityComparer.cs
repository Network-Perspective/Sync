using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Interactions
{
    public class InteractionEqualityComparer : IEqualityComparer<Interaction>
    {
        private static readonly IEqualityComparer<Employee> VertexEqualityComparer = Employee.EqualityComparer;

        public bool Equals(Interaction x, Interaction y)
        {
            if (x == null || y == null)
                return false;

            if (x.Timestamp != y.Timestamp)
                return false;

            if (!VertexEqualityComparer.Equals(x.Source, y.Source))
                return false;

            if (!VertexEqualityComparer.Equals(x.Target, y.Target))
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

            return HashCode.Combine(obj.Timestamp, VertexEqualityComparer.GetHashCode(obj.Source), VertexEqualityComparer.GetHashCode(obj.Target), obj.Type, userActionHash.ToHashCode(), obj.ChannelId);
        }
    }
}