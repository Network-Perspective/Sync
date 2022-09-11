namespace NetworkPerspective.Sync.Application.Domain.Meetings
{
    public class Combination
    {
        public string Source { get; }
        public string Target { get; }

        public Combination(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }
}