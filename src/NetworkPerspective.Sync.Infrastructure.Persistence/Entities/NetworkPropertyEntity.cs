using System;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Entities
{
    public class NetworkPropertyEntity
    {
        public long Id { get; set; }
        public Guid NetworkId { get; set; }
        public NetworkEntity Network { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}