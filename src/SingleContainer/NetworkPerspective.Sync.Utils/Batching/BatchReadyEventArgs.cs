using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Utils.Batching;

public class BatchReadyEventArgs<T>
{
    public IList<T> BatchItems { get; }
    public int BatchNumber { get; }

    public BatchReadyEventArgs(IEnumerable<T> batchItems, int batchNumber)
    {
        BatchItems = batchItems.ToList();
        BatchNumber = batchNumber;
    }
}