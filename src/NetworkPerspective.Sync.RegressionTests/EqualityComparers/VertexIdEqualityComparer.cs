using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NetworkPerspective.Sync.RegressionTests.Interactions
{
    internal class VertexIdEqualityComparer : IEqualityComparer<IDictionary<string, string>>
    {
        public bool Equals(IDictionary<string, string> x, IDictionary<string, string> y)
        {
            if (x == null || y == null)
                return false;

            if (x.Count != y.Count)
                return false;

            if (!x.Keys.ToHashSet().SetEquals(y.Keys))
                return false;

            foreach (var key in x.Keys)
            {
                if (x[key] != y[key])
                    return false;
            }

            return true;
        }

        public int GetHashCode([DisallowNull] IDictionary<string, string> obj)
        {
            var hashCode = new HashCode();

            foreach (var key in obj.Keys.OrderBy(x => x))
            {
                hashCode.Add(key);
                hashCode.Add(obj[key]);
            }

            return hashCode.ToHashCode();
        }
    }
}