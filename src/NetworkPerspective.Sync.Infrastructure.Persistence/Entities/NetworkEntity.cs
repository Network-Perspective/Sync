using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Entities
{
    public class NetworkEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<NetworkPropertyEntity> Properties { get; set; }
        public ICollection<SyncHistoryEntryEntity> SyncHistory { get; set; }
    }
}