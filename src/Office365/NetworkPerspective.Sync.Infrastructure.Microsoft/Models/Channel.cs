using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class Channel
    {
        public ChannelIdentifier Id { get; }
        public IEnumerable<string> UserIds { get; }

        public Channel(ChannelIdentifier id, IEnumerable<string> userIds)
        {
            Id = id;
            UserIds = userIds;
        }
    }
}