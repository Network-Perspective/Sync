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

        public ComparisonResult<T> Compare(IEnumerable<T> left, IEnumerable<T> right)
        {
            var leftList = left.ToList();
            var rightList = right.ToList();

            var onlyInLeft = leftList.Except(rightList, _equalityComparer);
            var onlyInRight = rightList.Except(leftList, _equalityComparer);
            var inBoth = left.Intersect(right, _equalityComparer);

            return new ComparisonResult<T>(onlyInLeft, onlyInRight, inBoth);
        }
    }
}
