using System;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Entities
{
    public class StatusLogEntity
    {
        public long Id { get; set; }
        public Guid NetworkId { get; set; }
        public NetworkEntity Network { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public int Level { get; set; }
    }
}