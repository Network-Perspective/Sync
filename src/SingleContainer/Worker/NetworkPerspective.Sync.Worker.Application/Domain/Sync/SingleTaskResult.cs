namespace NetworkPerspective.Sync.Worker.Application.Domain.Sync
{
    public class SingleTaskResult
    {
        public static SingleTaskResult Empty = new SingleTaskResult(0);
        public int InteractionsCount { get; }

        public SingleTaskResult(int interactionsCount)
        {
            InteractionsCount = interactionsCount;
        }
    }
}