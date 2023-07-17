namespace NetworkPerspective.Sync.Application.Domain.Combinations
{
    public class Combination<T>
    {
        public T Source { get; }
        public T Target { get; }

        public Combination(T source, T target)
        {
            Source = source;
            Target = target;
        }
    }
}