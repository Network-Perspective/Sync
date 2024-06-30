using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Entities
{
    public class ConnectorEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<ConnectorPropertyEntity> Properties { get; set; }
        public ICollection<SyncHistoryEntryEntity> SyncHistory { get; set; }
    }
}