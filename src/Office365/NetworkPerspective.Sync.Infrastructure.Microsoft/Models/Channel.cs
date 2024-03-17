using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class Channel
    {
        public string Id { get; }
        public string Name { get; }
        public Team Team { get; }
        public IEnumerable<string> UserIds { get; }

        public Channel(string id, string name, Team team, IEnumerable<string> userIds)
        {
            Id = id;
            Name = name;
            Team = team;
            UserIds = userIds;
        }
    }
}