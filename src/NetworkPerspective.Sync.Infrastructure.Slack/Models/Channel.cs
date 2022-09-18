namespace NetworkPerspective.Sync.Infrastructure.Slack.Models
{
    internal class Channel
    {
        public string Id { get; }
        public string Name { get; }
        public bool IsPrivate { get; }
        public Channel(string id, string name, bool isPrivate)
        {
            Id = id;
            Name = name;
            IsPrivate = isPrivate;
        }
    }
}