using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class Channel
    {
        public ChannelIdentifier Id { get; }
        public string Name { get; }
        public IEnumerable<string> UserIds { get; }

        public Channel(ChannelIdentifier id, string name, IEnumerable<string> userIds)
        {
            Id = id;
            Name = name;
            UserIds = userIds;
        }
    }
}