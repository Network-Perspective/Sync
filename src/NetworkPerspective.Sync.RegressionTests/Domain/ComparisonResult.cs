using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.RegressionTests.Domain
{
    internal class ComparisonResult<T>
    {
        public IList<T> OnlyInOld { get; }
        public IList<T> OnlyInNew { get; }
        public IList<T> InBoth { get; }

        public ComparisonResult(IEnumerable<T> onlyInOld, IEnumerable<T> onlyInNew, IEnumerable<T> inBoth)
        {
            OnlyInOld = onlyInOld.ToList();
            OnlyInNew = onlyInNew.ToList();
            InBoth = inBoth.ToList();
        }
    }
}