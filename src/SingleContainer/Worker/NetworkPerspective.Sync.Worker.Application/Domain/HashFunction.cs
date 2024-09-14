namespace NetworkPerspective.Sync.Worker.Application.Domain
{
    public class HashFunction
    {
        public delegate string Delegate(string input);

        public readonly static Delegate Empty = x => x;
    }
}