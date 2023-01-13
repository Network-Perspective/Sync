using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.RegressionTests.Domain;

namespace NetworkPerspective.Sync.RegressionTests.Services
{
    internal class Comparer<T>
    {
        private readonly IEqualityComparer<T> _equalityComparer;

        public Comparer(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
        }

        public ComparisonResult<T> Compare(IEnumerable<T> old, IEnumerable<T> @new)
        {
            var onlyInOld = old.Except(@new, _equalityComparer);
            var onlyInNew = @new.Except(old, _equalityComparer);
            var inBoth = old.Intersect(@new, _equalityComparer);

            return new ComparisonResult<T>(onlyInOld, onlyInNew, inBoth);
        }
    }
}
