using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.RegressionTests.Domain
{
    internal class ComparisonResult<T>
    {
        public IList<T> OnlyInLeft { get; }
        public IList<T> OnlyInRight { get; }
        public IList<T> InBoth { get; }

        public ComparisonResult(IEnumerable<T> onlyInLeft, IEnumerable<T> onlyInRight, IEnumerable<T> inBoth)
        {
            OnlyInLeft = onlyInLeft.ToList();
            OnlyInRight = onlyInRight.ToList();
            InBoth = inBoth.ToList();
        }
    }
}
