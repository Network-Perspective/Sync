using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class Chat
    {
        public string Id { get; }
        public IEnumerable<string> UserIds { get; }

        public Chat(string id, IEnumerable<string> userIds)
        {
            Id = id;
            UserIds = userIds;
        }
    }
}
