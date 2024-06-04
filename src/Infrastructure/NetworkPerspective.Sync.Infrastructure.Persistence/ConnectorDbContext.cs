using Microsoft.EntityFrameworkCore;

using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence
{
    public class ConnectorDbContext : DbContext
    {
        public DbSet<SyncHistoryEntryEntity> SyncHistoryEntities { get; set; }
        public DbSet<ConnectorEntity> NetworkEntities { get; set; }
        public DbSet<StatusLogEntity> StatusLogEntities { get; set; }
        public DbSet<SecretEntity> SecretEntities { get; set; }

        public ConnectorDbContext(DbContextOptions<ConnectorDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConnectorDbContext).Assembly);
    }
}